using System;
using System.Linq;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;
using Examine.LuceneEngine.Faceting;
using Examine.LuceneEngine.Indexing;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.Session;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using NUnit.Framework;
using Version = Lucene.Net.Util.Version;

namespace Examine.Test.Search
{
    /// <summary>
    /// Tests specific to Lucene criteria
    /// </summary>
    [TestFixture, RequiresSTA]
    public class LuceneSearchTests
    {
        [TearDown]
        public void Teardown()
        {
            DisposableCollector.Clean();
        }

        //TODO: Write tests for all 'LuceneSearch', 'LuceneQuery', 'Facets*', 'Wrap*' methods

        [Test]
        public void Can_Get_Lucene_Search_Result()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new
                        {
                            nodeName = "my name 1"
                        })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria("content");
                var filter = criteria.Field("nodeName", "name");
                var results = searcher.Find(filter.Compile());

                Assert.AreEqual(typeof(LuceneSearchResults), results.GetType());
            }
        }

        [Test]
        public void Can_Count_Facets()
        {
            //TODO: I'm not sure about passing the facet config into the indexer on ctor? 
            // in theory shouldn't we be able to specify this config when we search?

            var config = new FacetConfiguration();
            config.FacetExtractors.Add(new TermFacetExtractor("manufacturer"));
            config.FacetExtractors.Add(new TermFacetExtractor("resolution"));

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP" }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP" }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP" }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP" }),
                    new ValueSet(5, "content",
                        new { description = "hi there, hello world", manufacturer = "Canon", resolution = "12MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    //NOTE: This is optional, it is true by default!
                    //TODO: Should we change this to false by default?? I think so
                    .CountFacets(true)
                    .Field("description", "hello");
                
                var results = searcher.Find(filter.Compile());

                Assert.AreEqual(2, results.FacetCounts.FacetMap.FieldNames.Count());

                Assert.AreEqual(4, results.FacetCounts.Count(x => x.Key.FieldName == "manufacturer"));

                Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "canon").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "sony").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "pentax").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "nikon").Count);

                Assert.AreEqual(3, results.FacetCounts.Count(x => x.Key.FieldName == "resolution"));

                Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "4mp").Count);
                Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "2mp").Count);
                Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "12mp").Count);

                DebutOutputResults(results);
            }
        }

        [Test]
        public void Can_Count_Facets_Refs()
        {
            //TODO: I'm not sure about passing the facet config into the indexer on ctor? 
            // in theory shouldn't we be able to specify this config when we search?

            var config = new FacetConfiguration();
            config.FacetExtractors.Add(new TermFacetExtractor("manufacturer"));
            config.FacetExtractors.Add(new TermFacetExtractor("resolution"));

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP" }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP" }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP" }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP" }),
                    new ValueSet(5, "content",
                        new { description = "hi there, hello world", manufacturer = "Canon", resolution = "12MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    //NOTE: This is optional, it is true by default!
                    //TODO: Should we change this to false by default?? I think so
                    .CountFacets(true)
                    //NOTE: This is false by default
                    .CountFacetReferences(true)
                    .Field("description", "hello");

                var results = searcher.Find(filter.Compile());

                Assert.Fail("TODO: Find out why facet refs on each result is empty!");

                //Assert.AreEqual(2, results.FacetCounts.FacetMap.FieldNames.Count());

                //Assert.AreEqual(4, results.FacetCounts.Count(x => x.Key.FieldName == "manufacturer"));

                //Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "canon").Count);
                //Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "sony").Count);
                //Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "pentax").Count);
                //Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "nikon").Count);

                //Assert.AreEqual(3, results.FacetCounts.Count(x => x.Key.FieldName == "resolution"));

                //Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "4mp").Count);
                //Assert.AreEqual(1, results.FacetCounts.Single(x => x.Key.Value == "2mp").Count);
                //Assert.AreEqual(2, results.FacetCounts.Single(x => x.Key.Value == "12mp").Count);

                DebutOutputResults(results);
            }
        }


        [Test]
        public void Facet_Count_Is_Null_When_Disabled()
        {
            var config = new FacetConfiguration();
            config.FacetExtractors.Add(new TermFacetExtractor("manufacturer"));
            config.FacetExtractors.Add(new TermFacetExtractor("resolution"));

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP" }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP" }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP" }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP" }),
                    new ValueSet(5, "content",
                        new { description = "hi there, hello world", manufacturer = "Canon", resolution = "12MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    .CountFacets(false)
                    .Field("description", "hello");

                var results = searcher.Find(filter.Compile());

                Assert.IsNull(results.FacetCounts);
            }
        }

        [Test]
        public void Facet_Count_On_Result_Is_Null_When_Disabled()
        {
            var config = new FacetConfiguration();
            config.FacetExtractors.Add(new TermFacetExtractor("manufacturer"));
            config.FacetExtractors.Add(new TermFacetExtractor("resolution"));

            var analyzer = new StandardAnalyzer(Version.LUCENE_29);
            using (var luceneDir = new RAMDirectory())
            using (var indexer = new TestIndexer(luceneDir, analyzer, config))
            using (SearcherContextCollection.Instance)
            {
                indexer.IndexItems(
                    new ValueSet(1, "content",
                        new { description = "hello world", manufacturer = "Canon", resolution = "2MP" }),
                    new ValueSet(2, "content",
                        new { description = "hello something or other", manufacturer = "Sony", resolution = "4MP" }),
                    new ValueSet(3, "content",
                        new { description = "hello you guys", manufacturer = "Nikon", resolution = "12MP" }),
                    new ValueSet(4, "content",
                        new { description = "hello you cruel world", manufacturer = "Pentax", resolution = "4MP" }),
                    new ValueSet(5, "content",
                        new { description = "hi there, hello world", manufacturer = "Canon", resolution = "12MP" })
                    );

                ExamineSession.WaitForChanges();

                var searcher = new LuceneSearcher(luceneDir, analyzer);

                var criteria = searcher.CreateCriteria();
                var filter = criteria
                    //NOTE: This is false by default!
                    .CountFacetReferences(false)
                    .Field("description", "hello");

                var results = searcher.Find(filter.Compile());

                Assert.IsNull(results.First().FacetCounts);
            }
        }

        private void DebutOutputResults(ILuceneSearchResults results)
        {

            if (results.FacetCounts != null)
            {
                Console.WriteLine(" :: FACETS");
                foreach (var fc in results.FacetCounts)
                {
                    Console.WriteLine(fc.Key + " : " + fc.Count);
                }
            }

            foreach (var result in results)
            {

                Console.WriteLine(" :: RESULT :: " + result.GetHighlight("description"));
                if (result.FacetCounts != null)
                {
                    foreach (var fc in result.FacetCounts)
                    {
                        Console.WriteLine(fc.FieldName + " : " + fc.Count);
                    }
                }
            }
        }
    }
}