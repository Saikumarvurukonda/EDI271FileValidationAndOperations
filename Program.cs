using RDPCrystalEDILibrary;
using System.Collections;
using RDPCrystalEDILibrary.Docs.x4010;
using System.Collections.Generic;
using System.IO;
using System.Text;



var rulesFile = "Give Rules Path here";
var rulesFile5010 = "Give Rules Path here for 5010 Version";

DirectoryInfo d = new DirectoryInfo("C:\\git\\NEWEPIC\\CHES.Epic271\\Files"); //Assuming Test is your Folder

FileInfo[] Files = d.GetFiles("*.EDI"); //Getting Text files
string str = "";
EDIValidator ediValidatorFinal = null;


foreach (FileInfo file in Files)
{
    var ediValidator1 = fileValidate(file);
    if (ediValidator1.Passed)
    {
        if (ediValidatorFinal == null)
        {
            var dai = DateTime.Now.ToString("yyyyMMdd");
            var da = DateTime.Now.ToString("HHmm");
            var segmentBHT = ediValidator1.DataLoop.GetLoops("INTERCHANGE HEADER")[0].GetLoop("FUNCTIONAL GROUP").GetLoop("ST HEADER").GetSegment("BHT");
            segmentBHT.Elements[3].DataValue = dai.ToString();
            segmentBHT.Elements[4].DataValue = da.ToString();
            var segmentGS = ediValidator1.DataLoop.GetLoops("INTERCHANGE HEADER")[0].GetLoop("FUNCTIONAL GROUP").GetSegment("GS");
            segmentGS.Elements[4].DataValue = dai.ToString();
            segmentGS.Elements[5].DataValue = da.ToString();
            ediValidatorFinal = ediValidator1;
        }
        else
        {
            LightWeightLoop subscribers = ediValidatorFinal.DataLoop.GetLoops("INTERCHANGE HEADER")[0].GetLoop("FUNCTIONAL GROUP").GetLoop("ST HEADER");
            LightWeightLoop subscribers2 = ediValidator1.DataLoop.GetLoops("INTERCHANGE HEADER")[0].GetLoop("FUNCTIONAL GROUP").GetLoop("ST HEADER");
            LightWeightLoop loop = subscribers.GetLoop("2000A").GetLoop("2000B");
            LightWeightLoop loop2 = subscribers2.GetLoop("2000A").GetLoop("2000B");
            foreach (var item in loop2.Loops)
            {
                if (item.Name != "2100B")
                {
                    loop.Loops.Add(item);
                }
            }
        }
    }

}


if (ediValidatorFinal != null)
{

    LightWeightLoop subscribersFinal = ediValidatorFinal.DataLoop.GetLoops("INTERCHANGE HEADER")[0].GetLoop("FUNCTIONAL GROUP").GetLoop("ST HEADER");
    LightWeightLoop loopFinal = subscribersFinal.GetLoop("2000A").GetLoop("2000B");
    // This should run after combining all files
    subscribersFinal.GetLastLoop().GetSegment("SE").FirstElement.DataValue = (subscribersFinal.ToEDIString(new Delimiters()).Split("~").Length - 1).ToString();

    int count = -1;
    foreach (var item in loopFinal.GetLoops("2000C"))
    {
        if (count == -1)
        {
            count = Int32.Parse(item.GetSegment("HL").FirstElement.DataValue);
        }
        else
        {
            item.GetSegment("HL").FirstElement.DataValue = (++count).ToString();
        }

    }
    var deli = new Delimiters();
    deli.ElementTerminatorCharacter = '|';
    ediValidatorFinal.Validate();
    File.WriteAllText("C:\\git\\NEWEPIC\\CHES.Epic271\\Files\\Combined.271", ediValidatorFinal.DataLoop.ToEDIString(deli));
}



EDIValidator fileValidate(FileInfo file)
{
    EDIValidator ediValidator1 = new EDIValidator();
    ediValidator1.EDIRulesFile = rulesFile;
    ediValidator1.EDIFileType = FileType.X12;
    ediValidator1.EDISource = EDISource.DataString;
    ediValidator1.LoadValidatedData = true;
    ediValidator1.AutoDetectDelimiters = true;
    ediValidator1.TrackDownUnrecognizedLoops = false;
    ediValidator1.EDIRulesFile = rulesFile5010;
   
    var fileData = File.ReadAllBytes(file.FullName);
    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
    string strFile1 = encoding.GetString(fileData).Replace("\r", "").Replace("\n", "");
    ediValidator1.EDIDataString = strFile1;
    ediValidator1.Validate();
    return ediValidator1;
}
