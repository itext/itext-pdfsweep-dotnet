using System;
using System.Collections.Generic;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Test;

namespace iText.PdfCleanup {
    [NUnit.Framework.Category("IntegrationTest")]
    public class OverlapRatioTest : ExtendedITextTest {
        private static readonly String inputPath = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/OverlapRatioTest/";

        private static readonly String outputPath = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/OverlapRatioTest/";

        [NUnit.Framework.OneTimeSetUp]
        public static void Before() {
            CreateOrClearDestinationFolder(outputPath);
        }

        private static readonly double[][] coordinates = new double[][] { 
                //Areas with small line spacing
                new double[] { 1, 149, 700, 63.75, 10.75 }, new double[] { 1, 149, 640, 63.75, 10.75 }, new double[] { 1, 
            149, 520, 163.75, 50.75 }, 
                //Areas with big line spacing
                new double[] { 1, 149, 374, 63.75, 10.75 }, new double[] { 1, 149, 310, 63.75, 10.75 }, new double[] { 1, 
            149, 120, 163.75, 50.75 } };

        [NUnit.Framework.Test]
        public virtual void ExtractionWithoutSettingOverlapRatio() {
            String inputFile = inputPath + "redact_aspect_ratio_simple.pdf";
            String targetFile = outputPath + "wo_redact_aspect_ratio_simple_redact.pdf";
            String cmpFile = inputPath + "cmp_wo_redact_aspect_ratio_simple.pdf";
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputFile), new PdfWriter(targetFile));
            CleanUpProperties properties = new CleanUpProperties();
            PdfCleaner.CleanUp(pdfDoc, ConvertCleanupLocations(), properties);
            pdfDoc.Close();
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(targetFile, cmpFile, outputPath, "diff_");
            NUnit.Framework.Assert.IsNull(errorMessage);
        }

        [NUnit.Framework.Test]
        public virtual void ExtractionWithSettingOverlapRatio() {
            String inputFile = inputPath + "redact_aspect_ratio_simple.pdf";
            String targetFile = outputPath + "redact_aspect_ratio_simple_redact.pdf";
            String cmpFile = inputPath + "cmp_redact_aspect_ratio_simple.pdf";
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputFile), new PdfWriter(targetFile));
            CleanUpProperties properties = new CleanUpProperties();
            properties.SetOverlapRatio(0.35);
            PdfCleaner.CleanUp(pdfDoc, ConvertCleanupLocations(), properties);
            pdfDoc.Close();
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(targetFile, cmpFile, outputPath, "diff_");
            NUnit.Framework.Assert.IsNull(errorMessage);
        }

        [NUnit.Framework.Test]
        public virtual void ExtractionWithSettingOverlapRatioCloseTo0() {
            //In this test we expect it to behave as normal that everything that gets touched by the redaction \
            //area should be redacted.
            String inputFile = inputPath + "redact_aspect_ratio_simple.pdf";
            String targetFile = outputPath + "redact_aspect_ratio_0_simple_redact.pdf";
            String cmpFile = inputPath + "cmp_redact_aspect_ratio_0_simple.pdf";
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputFile), new PdfWriter(targetFile));
            CleanUpProperties properties = new CleanUpProperties();
            properties.SetOverlapRatio(0.0001);
            PdfCleaner.CleanUp(pdfDoc, ConvertCleanupLocations(), properties);
            pdfDoc.Close();
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(targetFile, cmpFile, outputPath, "diff_");
            NUnit.Framework.Assert.IsNull(errorMessage);
        }

        [NUnit.Framework.Test]
        public virtual void ExtractionWithSettingOverlapRatio1() {
            //In this sample we expect nothing to be redacted because of none of the items actually overlaps all of it.
            String inputFile = inputPath + "redact_aspect_ratio_simple.pdf";
            String targetFile = outputPath + "redact_aspect_ratio_1_simple_redact.pdf";
            String cmpFile = inputPath + "cmp_redact_aspect_ratio_1_simple.pdf";
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputFile), new PdfWriter(targetFile));
            CleanUpProperties properties = new CleanUpProperties();
            properties.SetOverlapRatio(1d);
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            // convertCleanupLocations();
            cleanUpLocations.Add(new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(20, 690, 263.75f, 40), ColorConstants
                .YELLOW));
            PdfCleaner.CleanUp(pdfDoc, cleanUpLocations, properties);
            pdfDoc.Close();
            CompareTool cmpTool = new CompareTool();
            String errorMessage = cmpTool.CompareByContent(targetFile, cmpFile, outputPath, "diff_");
            NUnit.Framework.Assert.IsNull(errorMessage);
        }

        private static IList<iText.PdfCleanup.PdfCleanUpLocation> ConvertCleanupLocations() {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            foreach (double[] coord in coordinates) {
                int pageNumber = (int)coord[0];
                double x = coord[1];
                double y = coord[2];
                double width = coord[3];
                double height = coord[4];
                iText.PdfCleanup.PdfCleanUpLocation location = new iText.PdfCleanup.PdfCleanUpLocation(pageNumber, new Rectangle
                    ((float)x, (float)y, (float)width, (float)height), ColorConstants.BLACK);
                cleanUpLocations.Add(location);
            }
            return cleanUpLocations;
        }
    }
}
