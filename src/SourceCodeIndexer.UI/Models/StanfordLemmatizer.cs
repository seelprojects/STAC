using System;
using System.IO;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using edu.stanford.nlp.util;
using java.util;
using SourceCodeIndexer.STAC.Stemmer;

namespace SourceCodeIndexer.UI.Models
{
    /// <summary>
    /// Uses Stanford Lemmatizer <see cref="http://nlp.stanford.edu/"/>
    /// </summary>
    public class StanfordLemmatizer : IStemmer
    {
        /// <summary>
        /// pipe line for nlp
        /// </summary>
        private readonly StanfordCoreNLP _pipeLine;

        // Annotations for parsing
        private readonly CoreAnnotations.SentencesAnnotation _sentencesAnnotation;
        private readonly CoreAnnotations.TokensAnnotation _tokensAnnotation;
        private readonly CoreAnnotations.LemmaAnnotation _lemmaAnnotation;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jarRootPath">Path to the folder with models extracted from 'stanford-corenlp-3.5.2-models.jar'</param>
        public StanfordLemmatizer( string jarRootPath)
        {
            if (!Directory.Exists(jarRootPath))
            {
                string fullPath = Path.GetFullPath(jarRootPath);
                throw new DirectoryNotFoundException("Folder(s) extracted from 'stanford-corenlp-3.5.2-models.jar' was not found in path: . " +
                                                     "-->" + fullPath + "<--. " +
                                                     "Please make sure correct path is listed in .config file.");
            }

            // Set properties required for lemma
            java.util.Properties props = new java.util.Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma");
            props.setProperty("ner.useSUTime", "0");

            // We should change current directory, so StanfordCoreNLP could find all the model files automatically
            string curDir = Environment.CurrentDirectory;
            Directory.SetCurrentDirectory(jarRootPath);
            _pipeLine = new StanfordCoreNLP(props);
            Directory.SetCurrentDirectory(curDir);

            // Instantiate annotation
            _sentencesAnnotation = new CoreAnnotations.SentencesAnnotation();
            _tokensAnnotation = new CoreAnnotations.TokensAnnotation();
            _lemmaAnnotation = new CoreAnnotations.LemmaAnnotation();
        }

        /// <summary>
        /// Gets stem text
        /// </summary>
        /// <param name="text">Text to stem</param>
        /// <returns>Text that is stemmed</returns>
        public string GetStemmedText(string text)
        {
            try
            {
                // Annotation
                var annotation = new Annotation(text);
                _pipeLine.annotate(annotation);

                // Sentence
                ArrayList sentences = annotation.get(_sentencesAnnotation.getClass()) as ArrayList;
                CoreMap sentence = sentences.get(0) as CoreMap;

                // Token
                ArrayList tokens = sentence.get(_tokensAnnotation.getClass()) as ArrayList;
                CoreLabel token = tokens.get(0) as CoreLabel;

                // Lemma
                string lemma = token.get(_lemmaAnnotation.getClass()).ToString();

                return lemma;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
