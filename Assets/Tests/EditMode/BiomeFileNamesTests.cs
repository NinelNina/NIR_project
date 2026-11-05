using NUnit.Framework;

namespace Level.Platforms.Tests
{
    public class BiomeFileNamesTests
    {
        [TestCase("Env_A1S1_MeadowGround_Left", BiomePart.Left)]
        [TestCase("Env_A1S1_MeadowGround_Middle_03", BiomePart.Middle)]
        [TestCase("Env_A1S1_MeadowGround_Right", BiomePart.Right)]
        [TestCase("Env_A1S1_MeadowGround_Top_Left", BiomePart.CrustLeft)]
        [TestCase("Env_A1S1_MeadowGround_Top_Middle_01", BiomePart.CrustMiddle)]
        [TestCase("Env_A1S1_MeadowGround_Bottom_Right", BiomePart.UnderRight)]
        [TestCase("Env_A1S1_MeadowGround_CornerOut_Left", BiomePart.CornerLeft)]
        [TestCase("Env_A1S1_MeadowGround_CornerOut_Right", BiomePart.CornerRight)]
        [TestCase("Env_A1S1_MeadowGround_Wall", BiomePart.Wall)]
        [TestCase("Env_A1S1_MeadowGround_Fill", BiomePart.Fill)]
        [TestCase("Env_A1S1_MeadowGround_EndSide", BiomePart.EndSide)]
        public void Parse_KnownPart_ReturnsPart(string fileName, BiomePart expected)
        {
            Assert.AreEqual(expected, BiomeFileNames.Parse(fileName));
        }

        [TestCase("ground_middle")]
        [TestCase("Env_A1S1_MeadowGround_Ceiling_01")]
        [TestCase("Env_Left")]
        [TestCase("")]
        public void Parse_UnknownName_ReturnsNone(string fileName)
        {
            Assert.AreEqual(BiomePart.None, BiomeFileNames.Parse(fileName));
        }
    }
}
