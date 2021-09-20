/*
This file is part of the iText (R) project.
Copyright (c) 1998-2021 iText Group NV
Authors: iText Software.

This program is offered under a commercial and under the AGPL license.
For commercial licensing, contact us at https://itextpdf.com/sales.  For AGPL licensing, see below.

AGPL licensing:
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO;
using iText.Commons.Actions;
using iText.Commons.Actions.Confirmations;
using iText.Commons.Actions.Processors;
using iText.Commons.Actions.Producer;
using iText.Commons.Actions.Sequence;
using iText.Commons.Utils;
using iText.IO.Source;
using iText.Kernel.Actions.Events;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.PdfCleanup.Actions.Event;
using iText.PdfCleanup.Autosweep;
using iText.Test;

namespace iText.PdfCleanup {
    public class CleanupLicenseEventsTest : ExtendedITextTest {
        private static readonly String INPUT_PATH = iText.Test.TestUtil.GetParentProjectDirectory(NUnit.Framework.TestContext
            .CurrentContext.TestDirectory) + "/resources/itext/pdfcleanup/PdfCleanUpToolTest/";

        private static readonly String OUTPUT_PATH = NUnit.Framework.TestContext.CurrentContext.TestDirectory + "/test/itext/pdfcleanup/PdfCleanUpToolTest/";

        private CleanupLicenseEventsTest.StoreEventsHandler handler;

        [NUnit.Framework.OneTimeSetUp]
        public static void BeforeClass() {
            CreateOrClearDestinationFolder(OUTPUT_PATH);
        }

        [NUnit.Framework.SetUp]
        public virtual void SetStoredEventHandler() {
            handler = new CleanupLicenseEventsTest.StoreEventsHandler();
            EventManager.GetInstance().Register(handler);
        }

        [NUnit.Framework.TearDown]
        public virtual void ResetHandler() {
            EventManager.GetInstance().Unregister(handler);
            handler = null;
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpRedactAnnotationsSendsCoreAndCleanUpEventTest() {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfDocument document = new PdfDocument(new PdfReader(INPUT_PATH + "absentICentry.pdf"), new PdfWriter(baos
                ));
            String oldProducer = document.GetDocumentInfo().GetProducer();
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation lineLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100
                , 560, 200, 30), ColorConstants.GREEN);
            cleanUpLocations.Add(lineLoc);
            PdfCleaner.CleanUpRedactAnnotations(document, new CleanUpProperties());
            document.Close();
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(2, events.Count);
            NUnit.Framework.Assert.AreEqual(ITextCoreProductEvent.PROCESS_PDF, events[0].GetEvent().GetEventType());
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[1].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(baos.ToArray())))) {
                String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCoreEvent(), GetCleanUpEvent
                    () }, oldProducer);
                NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpRedactAnnotationsWithAdditionalLocationSendsCoreAndCleanUpEventTest() {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfDocument document = new PdfDocument(new PdfReader(INPUT_PATH + "absentICentry.pdf"), new PdfWriter(baos
                ));
            String oldProducer = document.GetDocumentInfo().GetProducer();
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation lineLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100
                , 560, 200, 30), ColorConstants.GREEN);
            cleanUpLocations.Add(lineLoc);
            PdfCleaner.CleanUpRedactAnnotations(document, cleanUpLocations);
            document.Close();
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(2, events.Count);
            NUnit.Framework.Assert.AreEqual(ITextCoreProductEvent.PROCESS_PDF, events[0].GetEvent().GetEventType());
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[1].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(baos.ToArray())))) {
                String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCoreEvent(), GetCleanUpEvent
                    () }, oldProducer);
                NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpRedactAnnotationsWithNullLocationSendsCoreAndCleanUpEventTest() {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfDocument document = new PdfDocument(new PdfReader(INPUT_PATH + "absentICentry.pdf"), new PdfWriter(baos
                ));
            String oldProducer = document.GetDocumentInfo().GetProducer();
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = null;
            PdfCleaner.CleanUpRedactAnnotations(document, cleanUpLocations, new CleanUpProperties());
            document.Close();
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(2, events.Count);
            NUnit.Framework.Assert.AreEqual(ITextCoreProductEvent.PROCESS_PDF, events[0].GetEvent().GetEventType());
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[1].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(baos.ToArray())))) {
                String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCoreEvent(), GetCleanUpEvent
                    () }, oldProducer);
                NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpRedactAnnotationsWithSteamPramsSendsCleanUpEventTest() {
            Stream inputStream = new FileStream(INPUT_PATH + "absentICentry.pdf", FileMode.Open, FileAccess.Read);
            Stream outputStream = new FileStream(OUTPUT_PATH + "cleanUpRedactAnnotationsWithSteamPramsSendsCleanUpEventTest.pdf"
                , FileMode.Create);
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation lineLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100
                , 560, 200, 30), ColorConstants.GREEN);
            cleanUpLocations.Add(lineLoc);
            PdfCleaner.CleanUpRedactAnnotations(inputStream, outputStream, cleanUpLocations);
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(1, events.Count);
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[0].GetEvent().GetEventType());
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpRedactAnnotationsWithStreamArgumentsSendsCleanUpEventTest() {
            String @in = INPUT_PATH + "absentICentry.pdf";
            String @out = OUTPUT_PATH + "cleanUpRedactAnnotationsWithStreamArgumentTest.pdf";
            Stream file = new FileStream(@in, FileMode.Open, FileAccess.Read);
            Stream output = new FileStream(@out, FileMode.Create);
            PdfCleaner.CleanUpRedactAnnotations(file, output);
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(1, events.Count);
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[0].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(@out))) {
                using (PdfDocument inputDoc = new PdfDocument(new PdfReader(@in))) {
                    String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCleanUpEvent() }, inputDoc
                        .GetDocumentInfo().GetProducer());
                    NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
                }
            }
        }

        [NUnit.Framework.Test]
        public virtual void CleanUpWithStreamArgumentsSendsCleanUpEventTest() {
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation lineLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100
                , 560, 200, 30));
            cleanUpLocations.Add(lineLoc);
            String @in = INPUT_PATH + "page229.pdf";
            String @out = OUTPUT_PATH + "cleanUpWithStreamArgumentTest.pdf";
            Stream file = new FileStream(@in, FileMode.Open, FileAccess.Read);
            Stream output = new FileStream(@out, FileMode.Create);
            PdfCleaner.CleanUp(file, output, cleanUpLocations, new CleanUpProperties());
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(1, events.Count);
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[0].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(@out))) {
                using (PdfDocument inputDoc = new PdfDocument(new PdfReader(@in))) {
                    String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCleanUpEvent() }, inputDoc
                        .GetDocumentInfo().GetProducer());
                    NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
                }
            }
        }

        [NUnit.Framework.Test]
        public virtual void OpenDocumentAndCleanUpSendsCoreAndCleanUpEventsTest() {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfDocument document = new PdfDocument(new PdfReader(INPUT_PATH + "page229.pdf"), new PdfWriter(baos));
            String oldProducer = document.GetDocumentInfo().GetProducer();
            IList<iText.PdfCleanup.PdfCleanUpLocation> cleanUpLocations = new List<iText.PdfCleanup.PdfCleanUpLocation
                >();
            iText.PdfCleanup.PdfCleanUpLocation lineLoc = new iText.PdfCleanup.PdfCleanUpLocation(1, new Rectangle(100
                , 560, 200, 30), ColorConstants.GREEN);
            cleanUpLocations.Add(lineLoc);
            PdfCleaner.CleanUp(document, cleanUpLocations, new CleanUpProperties());
            document.Close();
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(2, events.Count);
            NUnit.Framework.Assert.AreEqual(ITextCoreProductEvent.PROCESS_PDF, events[0].GetEvent().GetEventType());
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[1].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(baos.ToArray())))) {
                String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCoreEvent(), GetCleanUpEvent
                    () }, oldProducer);
                NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
            }
        }

        [NUnit.Framework.Test]
        public virtual void AutoSweepHighlightSendsCoreEventsTest() {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfDocument document = new PdfDocument(new PdfReader(INPUT_PATH + "fontCleanup.pdf"), new PdfWriter(baos));
            String oldProducer = document.GetDocumentInfo().GetProducer();
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            PdfAutoSweepTools autoSweep = new PdfAutoSweepTools(strategy);
            autoSweep.Highlight(document);
            document.Close();
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(1, events.Count);
            NUnit.Framework.Assert.AreEqual(ITextCoreProductEvent.PROCESS_PDF, events[0].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(baos.ToArray())))) {
                String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCoreEvent() }, oldProducer
                    );
                NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
            }
        }

        [NUnit.Framework.Test]
        public virtual void AutoSweepCleanUpSendsCoreAndCleanUpEventsTest() {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfDocument document = new PdfDocument(new PdfReader(INPUT_PATH + "fontCleanup.pdf"), new PdfWriter(baos));
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            PdfCleaner.AutoSweepCleanUp(document, strategy);
            String oldProducer = document.GetDocumentInfo().GetProducer();
            document.Close();
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(2, events.Count);
            NUnit.Framework.Assert.AreEqual(ITextCoreProductEvent.PROCESS_PDF, events[0].GetEvent().GetEventType());
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[1].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(baos.ToArray())))) {
                String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCoreEvent(), GetCleanUpEvent
                    () }, oldProducer);
                NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
            }
        }

        [NUnit.Framework.Test]
        public virtual void AutoCleanWithStreamParamsSendsCleanUpEventTest() {
            String input = INPUT_PATH + "fontCleanup.pdf";
            String output = OUTPUT_PATH + "autoCleanWithStreamParamsSendsCleanUpEventTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            PdfCleaner.AutoSweepCleanUp(new FileStream(input, FileMode.Open, FileAccess.Read), new FileStream(output, 
                FileMode.Create), strategy);
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(1, events.Count);
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[0].GetEvent().GetEventType());
            using (PdfDocument outDoc = new PdfDocument(new PdfReader(output))) {
                using (PdfDocument inputDoc = new PdfDocument(new PdfReader(input))) {
                    String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCleanUpEvent() }, inputDoc
                        .GetDocumentInfo().GetProducer());
                    NUnit.Framework.Assert.AreEqual(expectedProdLine, outDoc.GetDocumentInfo().GetProducer());
                }
            }
        }

        [NUnit.Framework.Test]
        public virtual void AutoCleanWithLocationAndStreamParamsSendsCleanUpEventTest() {
            String input = INPUT_PATH + "fontCleanup.pdf";
            String output = OUTPUT_PATH + "autoCleanWithLocationAndStreamParamsSendsCleanUpEventTest.pdf";
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            IList<iText.PdfCleanup.PdfCleanUpLocation> additionalLocation = JavaUtil.ArraysAsList(new iText.PdfCleanup.PdfCleanUpLocation
                (1, new Rectangle(150, 650, 0, 0)));
            PdfCleaner.AutoSweepCleanUp(new FileStream(input, FileMode.Open, FileAccess.Read), new FileStream(output, 
                FileMode.Create), strategy, additionalLocation);
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(1, events.Count);
            NUnit.Framework.Assert.AreEqual(PdfSweepProductEvent.CLEANUP_PDF, events[0].GetEvent().GetEventType());
            using (PdfDocument outDoc = new PdfDocument(new PdfReader(output))) {
                using (PdfDocument inputDoc = new PdfDocument(new PdfReader(input))) {
                    String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCleanUpEvent() }, inputDoc
                        .GetDocumentInfo().GetProducer());
                    NUnit.Framework.Assert.AreEqual(expectedProdLine, outDoc.GetDocumentInfo().GetProducer());
                }
            }
        }

        [NUnit.Framework.Test]
        public virtual void AutoSweepTentativeCleanUpSendsCoreEventTest() {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            PdfDocument document = new PdfDocument(new PdfReader(INPUT_PATH + "fontCleanup.pdf"), new PdfWriter(baos));
            CompositeCleanupStrategy strategy = new CompositeCleanupStrategy();
            strategy.Add(new RegexBasedCleanupStrategy("leonard"));
            PdfAutoSweepTools autoSweep = new PdfAutoSweepTools(strategy);
            autoSweep.TentativeCleanUp(document);
            String oldProducer = document.GetDocumentInfo().GetProducer();
            document.Close();
            IList<ConfirmEvent> events = handler.GetEvents();
            NUnit.Framework.Assert.AreEqual(1, events.Count);
            NUnit.Framework.Assert.AreEqual(ITextCoreProductEvent.PROCESS_PDF, events[0].GetEvent().GetEventType());
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(baos.ToArray())))) {
                String expectedProdLine = CreateExpectedProducerLine(new ConfirmedEventWrapper[] { GetCoreEvent() }, oldProducer
                    );
                NUnit.Framework.Assert.AreEqual(expectedProdLine, pdfDocument.GetDocumentInfo().GetProducer());
            }
        }

        private static String CreateExpectedProducerLine(ConfirmedEventWrapper[] expectedEvents, String oldProducer
            ) {
            IList<ConfirmedEventWrapper> listEvents = JavaUtil.ArraysAsList(expectedEvents);
            return ProducerBuilder.ModifyProducer(listEvents, oldProducer);
        }

        private static ConfirmedEventWrapper GetCoreEvent() {
            DefaultITextProductEventProcessor processor = new DefaultITextProductEventProcessor(ProductNameConstant.ITEXT_CORE
                );
            return new ConfirmedEventWrapper(ITextCoreProductEvent.CreateProcessPdfEvent(new SequenceId(), null, EventConfirmationType
                .ON_CLOSE), processor.GetUsageType(), processor.GetProducer());
        }

        private static ConfirmedEventWrapper GetCleanUpEvent() {
            DefaultITextProductEventProcessor processor = new DefaultITextProductEventProcessor(ProductNameConstant.PDF_SWEEP
                );
            return new ConfirmedEventWrapper(PdfSweepProductEvent.CreateCleanupPdfEvent(new SequenceId(), null), processor
                .GetUsageType(), processor.GetProducer());
        }

        private class StoreEventsHandler : IBaseEventHandler {
            private readonly IList<ConfirmEvent> events = new List<ConfirmEvent>();

            public virtual IList<ConfirmEvent> GetEvents() {
                return events;
            }

            public virtual void OnEvent(IBaseEvent @event) {
                if (@event is ConfirmEvent) {
                    events.Add((ConfirmEvent)@event);
                }
            }
        }
    }
}
