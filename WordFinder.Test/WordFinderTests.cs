using WordFinder.Logic;

namespace WordFinder.Test
{
    public class WordFinderTests
    {
        private const int MAX_STRING_LENGTH = 64;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Should_Create_Word_Matrix()
        {
            string[] list = { "maximo", "maximo", "maximo", "maximo", "maximo" };
            //como hago para no tener que poner Logic.
            var wordFinder = new Logic.WordFinder(list);
            //rows should match
            Assert.That(5, Is.EqualTo(wordFinder.wordMatrix.Length));
            //cols should match
            foreach (var word in wordFinder.wordMatrix)
            {
                Assert.That(6, Is.EqualTo(word.Length));
            }
        }

        [Test]
        public void Should_Not_Create_Word_Matrix_Empty()
        {
            Assert.Throws(Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("word matrix can´t be empty"),() => new Logic.WordFinder(new List<string>()));
        }

        [Test]
        public void Should_Not_Create_Word_Matrix_Null()
        {
            Assert.Throws(Is.TypeOf<ArgumentNullException>(), () => new Logic.WordFinder(null));
        }

        [Test]
        public void Should_Not_Create_Word_Matrix_Row_Too_Big()
        {
            string incorrectWord = new('x', MAX_STRING_LENGTH + 1);
            string[] list = { incorrectWord, "maximo", "maximo", "maximo", "maximo" };

            Assert.Throws(Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("word matrix size invalid. Should be less than 64 cols and rows"), () => new Logic.WordFinder(list));
        }

        [Test]
        public void Should_Not_Create_Word_Matrix_Col_Too_Big()
        {
            List<string> matrix = new List<string>(); 
            
            for (int i = 0; i < MAX_STRING_LENGTH+1; i++) {
                matrix.Add("maximo");
            }

            Assert.Throws(Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("word matrix size invalid. Should be less than 64 cols and rows"), () => new Logic.WordFinder(matrix));
        }


        [Test]
        public void Should_Not_Create_Word_Matrix_Words_Not_Equals()
        {
            List<string> matrix = new List<string>();
            matrix.Add("maxi");
            for (int i = 0; i < MAX_STRING_LENGTH - 1; i++)
            {
                matrix.Add("maximo");
            }

            Assert.Throws(Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("every word from the matrix must have the same size"), () => new Logic.WordFinder(matrix));
        }



        [Test]
        public void DELETEtEST()
        {
            string[] list = { "asdfasdfmaximoasdf", "asdfasdfsolsolasdf", "asdfasdfeeeeeeasdf", "asdfasdfmarianasdf", "asdfasdfuuuuuuasdf" };
            var wordFinder = new Logic.WordFinder(list);

            var result = wordFinder.Find(new List<string> { "maximo", "mauricio", "maxi", "sol", "marian" });
            Assert.That(result.Count(), Is.EqualTo(3));
        }

    }
}