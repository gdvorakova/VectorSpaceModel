using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VectorSpaceModel.Data;
using VectorSpaceModel.Helpers.DocumentParsingHelpers;

namespace VectorSpaceModel
{
  internal class VectorSpaceModelIndex
  {
    // number of documents to remain 
    private readonly List<string> fTopicFiles;
    private readonly List<string> fDocumentFiles;

    private readonly string fIter = "0";
    private readonly string fRunId;
    private readonly string fOutputFile;

    private List<Document> fDocuments;
    private List<Query> fQueries;

    // list of word and count pairs, where the count represents the number of documents in which the word occured    
    private SortedDictionary<string, int> fDocumentFrequency;

    // list of word and list of doc indices pairs, where each index in the list is the pointer to the fDocuments list on the position of the document that contains the word    
    private SortedDictionary<string, List<int>> fPostingsList;

    public VectorSpaceModelIndex(List<string> topicFiles, List<string> documentFiles, string runId, string outputFile)
    {
      fTopicFiles = topicFiles;
      fDocumentFiles = documentFiles;
      fRunId = runId;
      fOutputFile = outputFile;
    }

    public void Create()            
    {
      CreateVocabulary();

      ComputeCosineScore computeScore;

      if (fRunId == "run-0")
      {
        CosineNormalize();
        computeScore = new ComputeCosineScore(ComputeCosineScoreRun0);
      }
      else
      {   
        LogarithmicTermWeighting();  
        //AugmentedTermWeighting();
        CosineNormalize();

        computeScore = new ComputeCosineScore(ComputeCosineScoreRun1);
      }

      var writer = new DocumentWriter(fOutputFile, fDocuments);

      fQueries.ForEach(q =>
      {
        var topScoreDocuments = computeScore(q);
        writer.PrintQueryResults(topScoreDocuments, q.QueryId, fIter, fRunId);
      });

      writer.Dispose();

    }

    private void AugmentedTermWeighting()
    {
      fDocuments.ForEach(doc => AugmentedTermWeighting(doc));
      fQueries.ForEach(q => AugmentedTermWeighting(q));
    }

    private void AugmentedTermWeighting(IDocument doc)
    { 
      var keys = new List<string>(doc.TermFrequency.Keys);
      foreach (var key in keys)
      {
        doc.TermFrequency[key] = 0.5 + (0.5 * doc.TermFrequency[key]) / doc.TermFrequency.Values.Max();
      }
    }   

    private void LogarithmicTermWeighting()
    {
      fDocuments.ForEach(doc => LogarithmicTermWeighting(doc));
      fQueries.ForEach(q => LogarithmicTermWeighting(q));
    }

    private void LogarithmicTermWeighting(IDocument doc)
    {
      var keys = new List<string>(doc.TermFrequency.Keys);
      foreach (var key in keys)
      {
        doc.TermFrequency[key] = 1 + Math.Log(doc.TermFrequency[key]);
      }
    }
                                                       
    private delegate Dictionary<int, double> ComputeCosineScore(Query query);

    private Dictionary<int, double> ComputeCosineScoreRun0(Query query)
    {
      var scores = new Dictionary<int, double>();

      foreach (var term in query.TermFrequency)
      {
        var tfWeightQuery = query.GetTf(term.Key);

        var docsWithTerm = new List<int>();

        if (fPostingsList.ContainsKey(term.Key))
        {
          docsWithTerm = fPostingsList[term.Key];
        }

        else
        {
          continue;
        }

        foreach (var docIndex in docsWithTerm)
        {
          var doc = fDocuments[docIndex];

          var tfWeightDoc = doc.GetTf(term.Key);

          if (scores.ContainsKey(docIndex))
          {
            scores[docIndex] += tfWeightDoc * tfWeightQuery;
          }
          else
          {
            scores.Add(docIndex, tfWeightDoc * tfWeightQuery);
          }
        }
      }

      return scores.OrderByDescending(key => key.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    private Dictionary<int, double> ComputeCosineScoreRun1(Query query)
    {
      var scores = new Dictionary<int, double>();

      foreach (var term in query.TermFrequency)
      {
        var tfQuery = query.GetTf(term.Key);
        double idf = 0;

        if (fDocumentFrequency.ContainsKey(term.Key))
        {
          idf = Math.Log(fDocuments.Count / fDocumentFrequency[term.Key]);

          // probabilistic idf:
          // idf = Math.Max(0, Math.Log((fDocuments.Count - fDocumentFrequency[term.Key]) / fDocumentFrequency[term.Key]));
        }
        else
        {
          idf = 0;
        }                                                                                                                   

        var tfidfQuery = tfQuery * idf; 

        var docsWithTerm = new List<int>();

        if (fPostingsList.ContainsKey(term.Key))
        {
          docsWithTerm = fPostingsList[term.Key];
        }

        else
        {
          continue;
        }

        foreach (var docIndex in docsWithTerm)
        {                                    
          var doc = fDocuments[docIndex];

          var tfDoc = doc.GetTf(term.Key);
          var tfidfDoc = tfDoc * idf;

          if (scores.ContainsKey(docIndex))
          {
            scores[docIndex] += tfidfDoc * tfidfQuery;
          }
          else
          {
            scores.Add(docIndex, tfidfDoc * tfidfQuery);
          }
        }
      }                  

      return scores.OrderByDescending(key => key.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    private void CreateVocabulary()
    {
      var fDocumentReader = new DocumentReader();
      var docIndex = -1;

      foreach (string doc in fDocumentFiles)
      {
        docIndex++;
        fDocumentReader.AddWordsFromDocToVocabulary(doc + ".vert", docIndex);
      }

      foreach (string topic in fTopicFiles)
      {
        fDocumentReader.AddWordsFromQueryToVocabulary(topic);
      }

      fDocumentFrequency = fDocumentReader.DocumentFrequency;
      fPostingsList = fDocumentReader.PostingsList;
      fDocuments = fDocumentReader.Documents;
      fQueries = fDocumentReader.Queries;
    }

    private void CosineNormalize()
    {
      fDocuments.ForEach(doc => CosineNormalize(doc));
      fQueries.ForEach(q => CosineNormalize(q));
    }

    private void CosineNormalize(IDocument doc)
    {
      double termFrequencySquaredSum = 0.0;

      foreach (var term in doc.TermFrequency)
      {
        termFrequencySquaredSum += term.Value * term.Value;
      }

      termFrequencySquaredSum = Math.Sqrt(termFrequencySquaredSum);

      var keys = new List<string>(doc.TermFrequency.Keys);
      foreach (var key in keys)
      {
        doc.TermFrequency[key] = doc.TermFrequency[key] / termFrequencySquaredSum;
      }
    }
  }
}
