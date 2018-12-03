using System.Collections.Generic;
using System.IO;
using VectorSpaceModel.Data;

namespace VectorSpaceModel.Helpers.DocumentParsingHelpers
{
  internal class DocumentReader
  {
    // number of documents that term occurs in
    public SortedDictionary<string, int> DocumentFrequency { get; private set; }
    // list of documents that each term occurs in
    // documents are identified by its index position in Documents list
    public SortedDictionary<string, List<int>> PostingsList { get; private set; }
    public List<Document> Documents;
    public List<Query> Queries;

    private readonly bool fDoIgnoreStopwords = false;

    private enum Column : int { NUMBER_OF_WORDS_IN_SENTENCE, FORM, LEMMA, TAG, NO_DEPENDENCY_PARENT, SYNTACTIC_RELATIONSHIP };

    private List<string> fStopWords = new List<string> { ",", "(", ")", ".", "/", ":", ";", "?", "[", "]", "_", ">", "<", "=", "+", "~", "ačkoli", "ahoj", "ale", "anebo", "ano", "asi", "aspoň", "během", "bez", "beze", "blízko", "bohužel", "brzo", "bude", "budeme", "budeš", "budete", "budou", "budu", "byl", "byla", "byli", "bylo", "byly", "bys", "čau", "chce", "chceme", "chceš", "chcete", "chci", "chtějí", "chtít", "chut'", "chuti", "co", "čtrnáct", "čtyři", "dál", "dále", "daleko", "děkovat", "děkujeme", "děkuji", "den", "deset", "devatenáct", "devět", "do", "dobrý", "docela", "dva", "dvacet", "dvanáct", "dvě", "hodně", "já", "jak", "jde", "je", "jeden", "jedenáct", "jedna", "jedno", "jednou", "jedou", "jeho", "její", "jejich", "jemu", "jen", "jenom", "ještě", "jestli", "jestliže", "jí", "jich", "jím", "jimi", "jinak", "jsem", "jsi", "jsme", "jsou", "jste", "kam", "kde", "kdo", "kdy", "když", "ke", "kolik", "kromě", "která", "které", "kteří", "který", "kvůli", "má", "mají", "málo", "mám", "máme", "máš", "máte", "mé", "mě", "mezi", "mí", "mít", "mně", "mnou", "moc", "mohl", "mohou", "moje", "moji", "možná", "můj", "musí", "může", "my", "na", "nad", "nade", "nám", "námi", "naproti", "nás", "náš", "naše", "naši", "ne", "ně", "nebo", "nebyl", "nebyla", "nebyli", "nebyly", "něco", "nedělá", "nedělají", "nedělám", "neděláme", "neděláš", "neděláte", "nějak", "nejsi", "někde", "někdo", "nemají", "nemáme", "nemáte", "neměl", "němu", "není", "nestačí", "nevadí", "než", "nic", "nich", "ním", "nimi", "nula", "od", "ode", "on", "ona", "oni", "ono", "ony", "osm", "osmnáct", "pak", "patnáct", "pět", "po", "pořád", "potom", "pozdě", "před", "přes", "přese", "pro", "proč", "prosím", "prostě", "proti", "protože", "rovně", "se", "sedm", "sedmnáct", "šest", "šestnáct", "skoro", "smějí", "smí", "snad", "spolu", "sta", "sté", "sto", "ta", "tady", "tak", "takhle", "taky", "tam", "tamhle", "tamhleto", "tamto", "tě", "tebe", "tebou", "ted'", "tedy", "ten", "ti", "tisíc", "tisíce", "to", "tobě", "tohle", "toto", "třeba", "tři", "třináct", "trošku", "tvá", "tvé", "tvoje", "tvůj", "ty", "určitě", "už", "vám", "vámi", "vás", "váš", "vaše", "vaši", "ve", "večer", "vedle", "vlastně", "všechno", "všichni", "vůbec", "vy", "vždy", "za", "zač", "zatímco", "ze", "že" };    

    public DocumentReader()
    {
      DocumentFrequency = new SortedDictionary<string, int>();
      PostingsList = new SortedDictionary<string, List<int>>();

      Documents = new List<Document>();
      Queries = new List<Query>();     
    }

    public void AddWordsFromDocToVocabulary(string fileName, int docIndex)
    {
      string docId = "";
      string docNo = "";
      var termFrequency = new Dictionary<string, double>();

      using (var sr = File.OpenText(fileName))
      {
        int length = 0;

        string line = string.Empty;
        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith("<DOCID>"))
          {
            int startIndex = line.IndexOf('>') + 1;
            int stopIndex = line.Substring(startIndex).IndexOf('<');

            docId = line.Substring(startIndex, stopIndex);
          }
          else if (line.StartsWith("<DOCNO>"))
          {
            int startIndex = line.IndexOf('>') + 1;
            int stopIndex = line.Substring(startIndex).IndexOf('<');

            docNo = line.Substring(startIndex, stopIndex);
          }
          // doc content
          else if (!line.StartsWith("<") && line != "")
          {
            string[] columns = line.Split();

            if (columns.Length != 6)
              continue;

            string form = columns[(int)Column.LEMMA];
            length++;

            if (!fDoIgnoreStopwords && fStopWords.Contains(form))
            {
              continue;
            }

            // add term to term frequency of this doc
            // add term to document frequency of the document collection
            if (DocumentFrequency.ContainsKey(form))
            {
              if (!termFrequency.ContainsKey(form))
              {
                DocumentFrequency[form]++;
                PostingsList[form].Add(docIndex);
                termFrequency[form] = 1;
              }
              else
              {
                termFrequency[form]++;
              }
            }
            else
            {
              DocumentFrequency[form] = 1;
              PostingsList.Add(form, new List<int>() { docIndex });
              termFrequency[form] = 1;
            }
          }
        }

        Documents.Add(new Document(docId, docNo, termFrequency, length));
      }
    }

    public void AddWordsFromQueryToVocabulary(string fileName)
    {
      string queryId = "";
      var termFrequency = new Dictionary<string, double>();

      using (var sr = File.OpenText(fileName))
      {
        int length = 0;
        string line = string.Empty;
        while ((line = sr.ReadLine()) != null)
        {
          if (line.StartsWith("<num>"))
          {
            line = sr.ReadLine();

            queryId = line;
          }
          // doc content
          else if (line.StartsWith("</title>"))
          {
            break;
          }
          else if (!line.StartsWith("<") && line != "")
          {
            string[] columns = line.Split();

            if (columns.Length != 6)
              continue;

            string form = columns[(int)Column.LEMMA];
            length++;

            if (!fDoIgnoreStopwords && fStopWords.Contains(form))
            {
              continue;
            }

            if (!termFrequency.ContainsKey(form))
            {
              termFrequency[form] = 1;
            }
            else
            {
              termFrequency[form]++;
            }
          }
        }             
        Queries.Add(new Query(queryId, termFrequency, length));
      }                                                          
    }

    public static List<string> LinesToList(string fileName)
    {
      var list = new List<string>();

      using (var sr = File.OpenText(fileName))
      {
        string line = string.Empty;
        while ((line = sr.ReadLine()) != null)
        {
          // doc content
          if (line != "")
          {
            list.Add(line);
          }
        }
      }

      return list;
    }
  }
}
