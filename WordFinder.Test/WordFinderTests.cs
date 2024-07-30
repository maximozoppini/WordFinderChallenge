

using Microsoft.Extensions.Configuration;

namespace WordFinder.Test
{
    public class WordFinderTests
    {
        private const int MAX_STRING_LENGTH = 64;
        private IConfiguration _config;

        [SetUp]
        public void Setup()
        {
            // Set up configuration
            this._config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        }

        [Test]
        public void Should_Create_Word_Matrix()
        {
            string[] list = { "maximo", "maximo", "maximo", "maximo", "maximo" };
            //como hago para no tener que poner Logic.
            var wordFinder = new Logic.WordFinder(_config, list);
            //rows should match
            Assert.That(5, Is.EqualTo(wordFinder.WordMatrix.Length));
            //cols should match
            foreach (var word in wordFinder.WordMatrix)
            {
                Assert.That(6, Is.EqualTo(word.Length));
            }
        }

        [Test]
        public void Should_Not_Create_Word_Matrix_Empty()
        {
            Assert.Throws(Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("word matrix can´t be empty"), () => new Logic.WordFinder(_config,new List<string>()));
        }

        [Test]
        public void Should_Not_Create_Word_Matrix_Null()
        {
            Assert.Throws(Is.TypeOf<ArgumentNullException>(), () => new Logic.WordFinder(_config, null));
        }

        [Test]
        public void Should_Not_Create_Word_Matrix_Row_Too_Big()
        {
            string incorrectWord = new('x', MAX_STRING_LENGTH + 1);
            string[] list = { incorrectWord, "maximo", "maximo", "maximo", "maximo" };

            Assert.Throws(Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("word matrix size invalid. Should be less than 64 cols and rows"), () => new Logic.WordFinder(_config, list));
        }

        [Test]
        public void Should_Not_Create_Word_Matrix_Col_Too_Big()
        {
            List<string> matrix = new List<string>();

            for (int i = 0; i < MAX_STRING_LENGTH + 1; i++)
            {
                matrix.Add("maximo");
            }

            Assert.Throws(Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo("word matrix size invalid. Should be less than 64 cols and rows"), () => new Logic.WordFinder(_config, matrix));
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
                .And.Message.EqualTo("every word from the matrix must have the same size"), () => new Logic.WordFinder(_config, matrix));
        }



        [Test]
        public void Should_Create_Matrix_Return_Correct_Results()
        {
            string[] list = { "enmcsolrdsgi", "bwaeqvplcoxp", "smxmaximolmq", "exiofrxbzqwu", "tnitljbyzxdl", "ytamqsagqzsl", "ctpmarianaob", "marianellaln" };
            var wordFinder = new Logic.WordFinder(_config, list);
            
            var result = wordFinder.Find(new List<string> { "maximo", "laura", "maxi", "sol", "marian", "sole" });
            Assert.That(result.Count(), Is.EqualTo(4));
            Assert.That(result.First().Equals("sol"));
            Assert.That(result.Last().Equals("maximo"));
            Assert.That(result, Does.Not.Contain("sole"));
        }


        [Test]
        public void Should_Create_Matrix_Return_Correct_Results_StressTest()
        {
            string[] list =
                {
                "SSABTUHVSKBRYANTONRZAKOSAMUELPIGYZWXGFSKKHBGIBGOPRKKCLBYHXQZXTJA",
                "ZENVQQOKGPVAPOJMWWIMARTINMJEFFREYGPGANPTFYBSCOTTLNPRUAKTCMZLFINL",
                "THDSKDOUGLASVRCPNEBWIPCSAGHAECDPEWXWDISYBWLBIFDAMZDKGRPKFJQOUGHE",
                "TPRRAIUICITOSEHIBSYEFMMXZFODIIAPWQYBCCJERWYRSVMHOCALHRDTFWBPCKJX",
                "DQEOOSHYBTHYSXYQGFPWBXPSNTHPETERZMJTKHBPISPYVRKBQZCZUYXRYDBBFPAA",
                "EWWEDDWONKEVINMTQTEHIGATMPCOOLQIXDDHWOJFAYXAYVTMNNDHCEDAGACWCLMN",
                "NMRUMQLCCHDYEHASAJEJCYLCDULCQJAMESQOHLEBNDONSIZALEXANDERNFABVRED",
                "NDUBBVGAPTAXMWRXGTSPPBZXACEPSYVHTMDMVAEUYOZNZIERGHJBZACHARYIOJSE",
                "IRBDONALDBUULCTRJJVNNEKGCOOEACZOSBIAJSDTEBJSDGPGBWOTCNCZGPNLRONR",
                "SYPPMRRWWDFMIVIXUHTUHTQBQRUWLYRFIMHSOQWOQGUMUIVRMWNYSJSHABIJHSGG",
                "JGUJGIYRLJBXPPNYSGAERICSUKWFALJYFQFMEMACDXRDJERRYTATPRYANKZSHHHJ",
                "GNJEOPPWXZLFSGWBTEJKJYBYBLJMNCENEDCYTCRAERMHKDTBTNTGMWBKXYNRYUXJ",
                "TGBFDMVHCYSPCNLBIXAMLYYZZIGFLNYSNLZKDHDIFHDJBHVVRBHMRELELFFLGAAE",
                "LSBFRSJOEFWMRKENNETHYFQTAPFQLAKSUSVDTONATPNSBZDUPKAHSTVNUDJEREMY",
                "FDHRUAOYYUCCOZIEODDXNKAMJUVXFWUJACOBGVELZUKEPDVAIXNAOHJNFEHZUXJQ",
                "KNTEQMRRPBOJNQERROIHRIRXLWKHXIIKCFGUYRRYBEBGEORGEYONLAVENCTOXJEB",
                "SAUYEUDWQBCAAXAPLDLRRSRIORNHTLGIUMDUCDIMROAVXEGDNATBJNJTGSJJNABF",
                "CZJBPEADTVHXLBNPPQEITBTWLCLDDLJUCOLHPRCAUNEESCPGHECHAWZHVIEJNMCJ",
                "OKWTNLNALCDMDHLPWNDBBRQHVEQDDIKTMICHAELRCLZXOAVRIJOSEPHHTTRMVEUR",
                "TMYYEDINUPVBYZTUPVAVEWCPWASOZEDHZLSZUFQKETORNSWDBFNDPJXRHWEHHSMM",
                "TUMOBENINQNWCEPTETPQNBDZMRTNRDTYNVJORDANNSPCLBEKFPGUYSPYUSMMXRCL",
                "YLYNDOBEMZQVRARKENZHJWIXIEQACTUGUUFXYHAROLDVWIWEYYBQANTHONYRXVID",
                "VIZRBAJLZSADAMJTMKYGAGINZOBLMVTIMOTHYWTDILKYYLAVYZAEIILAALUTLTWD",
                "QYDYLANAVUWAMPYXBOJFMXWLYRIDFUQGDDOXDCAYDEATYLLIXNATHANYHQTZXEPO",
                "RNGBFQUVRNDQNQWYMFPSIHTVHOEHUXEWVAOXSUQLQWXHVYTNAIHRGFGECIDHXTWC",
                "HMPNDVYHARRYJXTFYRKJNDVLVGTVWDSFJVFMEDYAYIYVBUEOLQVGJLCDAVFZFPZM",
                "SJAQGGWVEHKLUJQNPSNJXARLDEPIPKNQNIVMKCDNMSZNMKRTSUHUVAAOGLXPDUQX",
                "FKAIYEIHOVACBNWGJRECKCYUTRWJRINKYDGXAXQBYPQMUABILLYKUPRWFLDTJBRU",
                "FOEXSILPAULDLXNTJCPXHLQZKMXASSXNYUYLBNZYOSPYBLORBEAVWALPUIVJRTDM",
                "GNFIRQLIMXTDRMRWIROZYEDWARDSVLSFQCLMLORONALDDBJDBZJUSBJFKTTGZTOQ",
                "SXHJBNIOICGGYMZSOBDHDIJPJEBOEFHDJXGVUATDJSTTUEJBCDMWTGRTJKGXVMKK",
                "LJWMECATXHXFVNJTCZZRFVAELZKNJESSEXOZCHRISTIANRPGJYUXEKLJUIYBBASB",
                "LBLEBHMWILLIAMDMTUQOBRTIFOAZQZKNMFGAKJWRIHMMVTUKFEYOVWFTPHFLMOFV",
                "NQBTRRMABJZSQNDDMNFEXYDGNOAHFMCHKHEVHAKQHVOADAVIDYJLERUNYAACZODN",
                "TSHGAIIOHGJERDBWTSCOYADXWCDAAAWSVQOALCGSHBPAZGDQOHWINFBYDERUZTSB",
                "UHCHNSCPQWUDAVSKGUFBPNPPZJKXWXBGSHRUZKRFFDDSRZYSWJQQINONVSWMFJTB",
                "IMQTDTHVVVZTNFNQXCCHARLESOIUFQPVNDGSIUZPHSSKVFITHOMASZTTYVPRMDNR",
                "BICCOIAMTQMBTAMZGARYIMKMUSMQZKYLESETFAEIKBGFOVDZIYXELANDREWLUDHF",
                "QDWWNAEKCECVHACSSOVZVKQOCEFNHICMXENIWAJKTMJACKIAIEFYCCRXRZSCWFKG",
                "GRGGYNLEHILVOSTTDNFISYPFBPSKSSGLIBMNLGOCXJNCLWMWDCRGBHXPMOQEGABM",
                "ZIROGERKFGNRNHLWNDZEDLPCZHMYMAKAFENXGCMIUXUQFRANKYATVAKVVSDANIEL",
                "SYPWCGZFKZVXYDUVDHGUDEUJONATHANMBCAPOGAJBCLSILARRYNKSRQZSVHDIMDY",
                "QIXNWPPRIGIVTKFLXKUUMQXKGLDRKNEXUQCARLKYGTPMHEUSXOKKQYDCGGMTFVLS",
                "RGFFMANPMAXSTEPHENNHUFWIYMPZAHPJBRIANNXMWNCMIGEJWILLIEAKNOQIJETH",
                "OBWNAUQLVIPEFNNPVRBJHLYVWVZGMNKXEKWDDPFNHJJZIFJQVNVDVYMIRMGMXRJT",
                "FSKDTLGAEKTJELKPVMPRDTIFMATTHEWPDQVUKXEIAQUMSKJILWBNVESKIWAOFNTR",
                "PPYYVCNPUZHWYJIERLUOXQTIVBHOCNJTBKMFCWALTERMMYHXJWAXNZTXYVZTJDFI",
                "HAEDKSLOQBTGIISTFBHMFUBRPFZHPQOYFYXUQFJASONHTDEHGDDSDENNISMHEJIJ",
                "QWMBRUCEYONZXWMEHIDJMPJCAUVOALILFEBRANDONCYHYSNMIDAHEUXVGTMYYKXX",
                "JKQJVPQUVLLAOPARUGIRIKNRMNNQTDNEUSPTUEPKEBHAQORGZWMANUORAUSTINHN",
                "VBSYZDZXQMBLYCQIRTLKGFAJSYVSRFXRVQEMGDKNOMIUJFYCFITRDFVFGRMNKUDC",
                "IZTJYJGZMARKLZABWQTVZFTOYUHFICPAZNXVZTVENICHOLASLNORLIMSFTMZJJUH",
                "ZSAAQQUEEUDRKJOLEWISXLHHZEOICNOJCOHHBROTRKXALBERTAHYKZHPPNHLEQGA",
                "WQDMDTNCWHLWAOTVERLQZDANJFOJKDOPNBBEORWPQDTYLERAAJCWMLGSKRWGSXOR",
                "AGOEXVHYVWHLQSCCDEUELBNSYOSQWRNQIOSJAIWJWZPWNDIFYEDCLOQWBMWKSEGL",
                "UZUSOLEVKQVVAEJOSHUASXZOCXPZZCXROBERTLMRXZYAKJOHNOTLJIEFXBIXEFME",
                "POGHXJIBAVLXBYSQOVUBNKSTEVENDWVYATAGSPAQOVJAFBENJAMINYRRFWPNRFFS",
                "RVLQNEYUHJPATRICKDEAHSFAZZTAOJEHEYKKAFTZSPIRWCETHANRICHARDJWOMQX",
                "DKAKRRHAJOFHFZNNIFLBMTWMGBFGCAPBJDCIHBTTZIKOABCOEQJIDNHNJWJYBKXT",
                "MXSXBRQXAYONCALANLHGEEBJFAOSDGFYAOKQGMHIHIUNJIHHPMACANASDWMTEOLG",
                "WOAKYYYHMUSTINBALMMPRPRDBMAASCCZMERRQOELKPIWTZELGDCHFRRZXVDPRAVG",
                "DXHBJIWAEZZLUIHSMZICPHBMGNJHRUHPEAARONWCDUPZGJNVHPOABKOQEQNGTSUV",
                "TWZCPNCFSGROWZDPMDEMBEFREHPONWUNSHVZYYNFIBCDGXWTQVBRXLLOUKGZIEWE",
                "RRBCTAOHQZJPPBEDNUOGCNRMHENRYXNKPQIZRFQAHOCCSZDSJTNDTQDSBLJOSEOE"
            };
            var wordFinder = new Logic.WordFinder(_config, list);
            var names = new List<string>
                {
                    "JAMES", "JHON", "ROBERT", "MICHAEL", "WILLIAM", "DAVID", "RICHARD", "JOSEPH", "CHARLES", "TOMAS",
                    "DANIEL", "MATTHEU", "ANTHONY", "MARX", "DONAL", "STEVEN", "PAUL", "ANDREW", "JOSHUA", "KENNETH",
                    "KEVIN", "BRIAN", "GEORGE", "EDWARD", "RONALD", "TIMOTHY", "JASON", "JEFFREY", "RYAN", "JACOB"
                };
            var result = wordFinder.Find(names);
            Assert.That(result.Count(), Is.EqualTo(10));
            Assert.That(result.First().Equals("JAMES"));
            Assert.That(result.ElementAt(1).Equals("RYAN"));
            Assert.That(result, Does.Not.Contain("MARX"));
        }
    }
}