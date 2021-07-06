using System;
using System.Linq;
using utauPlugin;
using System.Text.RegularExpressions;
using System.Configuration;

namespace AutoVelocity
{
    class Program
    {
        static void Main(string[] args)
        {
            UtauPlugin utauPlugin = new UtauPlugin(args[0]);
            utauPlugin.Input();

            // initialize all variables
            int baseVelocity = Convert.ToInt32(ConfigurationManager.AppSettings["baseVelocity"]); // velocity on a quarter note if the tempo was 120 (default velocity)
            float baseTempo = float.Parse(ConfigurationManager.AppSettings["baseTempo"]); // usually the tempo at which the voicebank was recorded, almost always 120
            int velocityAmount = Convert.ToInt32(ConfigurationManager.AppSettings["velocityAmount"]); // how much velocity is added or subtracted to notes smaller or larger than a quarter note
            bool doManualVelocity = bool.Parse(ConfigurationManager.AppSettings["doManualVelocity"]); // enables manual velocity variables (doesnt do anything at the moment)
            int manualVelocity = Convert.ToInt32(ConfigurationManager.AppSettings["manualVelocity"]);
            int qNoteLength = Convert.ToInt32(ConfigurationManager.AppSettings["qNoteLength"]); // length of a quarter note, shouldn't really be changed but whatever
            bool endingVelocity = bool.Parse(ConfigurationManager.AppSettings["endingVelocity"]); // whether or not the plugin will edit the velocity of ending notes
            bool beginningVelocity = bool.Parse(ConfigurationManager.AppSettings["beginningVelocity"]); // whether or not the plugin will edit the velocity of starting -CV or -CC notes
            bool modSmooth = bool.Parse(ConfigurationManager.AppSettings["modSmooth"]); // whether or not the plugin will set mod to 0 on Phase 2
            string consonantScope = ConfigurationManager.AppSettings["consonantScope"]; // Scope of consonants to look for. Defaults to VCCV English style.
            string vowelScope = ConfigurationManager.AppSettings["vowelScope"]; // Same as above, except with vowels. & -> &amp; due to it needing an escape character.

            if (doManualVelocity == true)
            {
                Console.Write("manualVelocity=");
                manualVelocity = Int32.Parse(Console.ReadLine());
            }

            // Print console information
            Console.WriteLine("AutoVelocity v1.1.0 by vocatart\n\n");
            Console.WriteLine("CONFIG VARIABLES\n");
            Console.WriteLine("baseVelocity=" + baseVelocity);
            Console.WriteLine("baseTempo=" + baseTempo);
            Console.WriteLine("velocityAmount=" + velocityAmount);
            Console.WriteLine("doManualVelocity=" + doManualVelocity);
            if (doManualVelocity == true)
            {
                Console.WriteLine("manualVelocity=" + manualVelocity);
            }
            Console.WriteLine("endingVelocity=" + endingVelocity);
            Console.WriteLine("beginningVelocity=" + beginningVelocity);
            Console.WriteLine("modSmooth=" + modSmooth);
            Console.WriteLine("qNoteLength=" + qNoteLength);
            Console.WriteLine("consonantScope=" + consonantScope);
            Console.WriteLine("vowelScope=" + vowelScope  + "\n");

            Console.WriteLine("Please select mode:\n" +
                "(Phase 1) Calculate velocity of CV notes (1)\n" +
                "(Phase 2) Calculate velocity of other notes (2)\n" +
                "(**Experimental**) Adjust velocity of all notes (3)");

            Console.Write("1/2/3: ");
            int velocityMode = Int32.Parse(Console.ReadLine());

            if (velocityMode == 1) // Phase 1 velocity setting for CV notes
            {
                foreach (Note note in utauPlugin.note)
                {
                    // woohoooooo variables oh mannnn oh yeahhhh
                    float noteTempo = note.GetTempo();
                    int noteLength = note.GetLength();
                    float tempoDifference = Math.Abs(noteTempo - baseTempo);
                    bool velocityInvert = ((noteTempo - baseTempo) < 0) ? true : false;
                    int qNoteVelocity = (int)(tempoDifference + baseVelocity);

                    if (doManualVelocity == false)
                    {
                        qNoteVelocity = (int)(tempoDifference + baseVelocity);
                    }
                    else
                    {
                        qNoteVelocity = manualVelocity;
                    }

                    int velocityRequest = 0;
                    int velocity = (velocityRequest * velocityAmount) + qNoteVelocity;
                    float loopAmount = qNoteLength / noteLength;
                    float largeLoopAmount = noteLength / qNoteLength;
                    string regionNumber = note.GetNum();

                    // regex to define -CV notes
                    Regex findBeginningCV = new Regex("-(.*)");
                    bool containsBeginningCV = findBeginningCV.IsMatch(note.GetLyric());

                    // regex to define NON CV notes
                    Regex findCV = new Regex("[" + consonantScope + "](.*)[" + vowelScope + "](.*)");
                    bool isCV = findCV.IsMatch(note.GetLyric());

                    if (containsBeginningCV == true & beginningVelocity == false) // catches notes that are -CV notes
                    {
                        note.SetVelocity(note.GetVelocity());
                    }
                    else if (isCV == false)
                    {
                        note.SetVelocity(note.GetVelocity());
                    }
                    else if (noteLength == qNoteLength & velocityInvert == false) // if note is quarter note & is above base tempo
                    {
                        note.SetVelocity(qNoteVelocity);
                    }
                    else if (noteLength == qNoteLength & velocityInvert == true) // if note is quarter note & is below base tempo
                    {
                        note.SetVelocity(baseVelocity);
                    }
                    else if (noteLength < qNoteLength & velocityInvert == false) // if note is smaller than quarter note
                    {
                        for (int i = 0; i < loopAmount; i++)
                        {
                            velocityRequest++;
                            velocity = ((velocityRequest - 1) * velocityAmount) + qNoteVelocity;
                            note.SetVelocity(velocity);
                        }

                    }
                    else if (noteLength > qNoteLength & velocityInvert == false) // if note is larger than quarter note
                    {
                        for (int i = 0; i < largeLoopAmount; i++)
                        {
                            velocityRequest--;
                            velocity = ((velocityRequest + 1) * velocityAmount) + qNoteVelocity;
                            note.SetVelocity(velocity);
                        }
                    }

                    if (modSmooth == true)
                    {
                        note.SetMod(0);
                    }

                    Console.WriteLine("regionNumber=[" + regionNumber + "]");
                    Console.WriteLine("noteLyric=[" + note.GetLyric() + "]");
                    Console.WriteLine("velocityRequest=" + velocityRequest);
                    Console.WriteLine("velocity=" + velocity);
                    Console.WriteLine("noteTempo=" + noteTempo);
                    Console.WriteLine("noteLength=" + noteLength);
                    Console.WriteLine("tempoDifference=" + tempoDifference);
                    Console.WriteLine("velocityInvert=" + velocityInvert);
                    Console.WriteLine("qNoteVelocity=" + qNoteVelocity + "\n");

                    System.Threading.Thread.Sleep(25);

                }

            }

            if (velocityMode == 2) // Phase 2 velocity setting for all other note types
            {

                foreach (Note note in utauPlugin.note)
                {
                    // variables once Again omg
                    string tempNoteNum = note.GetNum();
                    string noteNum = null;

                    if (tempNoteNum.All(char.IsDigit) == true)
                    {
                        noteNum = note.GetNum();
                    }
                    else
                    {
                        continue;
                    }

                    int previousVelocity = note.Prev.GetVelocity();

                    // regex to define CV notes
                    Regex findCV = new Regex("[" + consonantScope + "](.*)[" + vowelScope + "](.*)");
                    bool isCV = findCV.IsMatch(note.GetLyric());

                    // regex to define ending notes
                    Regex findEnding = new Regex("(.*)-");
                    bool isEnding = findEnding.IsMatch(note.GetLyric());

                    if (isEnding == true & endingVelocity == false)
                    {
                        note.SetVelocity(note.GetVelocity());
                    }
                    else if (isCV == false)
                    {
                        note.SetVelocity(previousVelocity);
                    }
                    else
                    {
                        continue;
                    }

                    if (modSmooth == true)
                    {
                        note.SetMod(0);
                    }

                    int velocity = note.GetVelocity();

                    Console.WriteLine("regionNumber=[" + noteNum + "]");
                    Console.WriteLine("noteLyric=[" + note.GetLyric() + "]");
                    Console.WriteLine("velocity=" + velocity);
                    Console.WriteLine("previousVelocity=" + previousVelocity + "\n");
                    System.Threading.Thread.Sleep(25);
                }
            }

            if (velocityMode == 3) // EXPERIMENTAL velocity setting for all notes
            {
                foreach (Note note in utauPlugin.note) // CV
                {
                    // woohoooooo variables oh mannnn oh yeahhhh
                    float noteTempo = note.GetTempo();
                    int noteLength = note.GetLength();
                    float tempoDifference = Math.Abs(noteTempo - baseTempo);
                    bool velocityInvert = ((noteTempo - baseTempo) < 0) ? true : false;
                    int qNoteVelocity = (int)(tempoDifference + baseVelocity);

                    if (doManualVelocity == false)
                    {
                        qNoteVelocity = (int)(tempoDifference + baseVelocity);
                    }
                    else
                    {
                        qNoteVelocity = manualVelocity;
                    }

                    int velocityRequest = 0;
                    int velocity = (velocityRequest * velocityAmount) + qNoteVelocity;
                    float loopAmount = qNoteLength / noteLength;
                    float largeLoopAmount = noteLength / qNoteLength;
                    string regionNumber = note.GetNum();

                    // regex to define -CV notes
                    Regex findBeginningCV = new Regex("-(.*)");
                    bool containsBeginningCV = findBeginningCV.IsMatch(note.GetLyric());

                    // regex to define NON CV notes
                    Regex findCV = new Regex("[" + consonantScope + "](.*)[" + vowelScope + "](.*)");
                    bool isCV = findCV.IsMatch(note.GetLyric());

                    if (containsBeginningCV == true & beginningVelocity == false) // catches notes that are -CV notes
                    {
                        note.SetVelocity(note.GetVelocity());
                    }
                    else if (isCV == false)
                    {
                        note.SetVelocity(note.GetVelocity());
                    }
                    else if (noteLength == qNoteLength & velocityInvert == false) // if note is quarter note & is above base tempo
                    {
                        note.SetVelocity(qNoteVelocity);
                    }
                    else if (noteLength == qNoteLength & velocityInvert == true) // if note is quarter note & is below base tempo
                    {
                        note.SetVelocity(baseVelocity);
                    }
                    else if (noteLength < qNoteLength & velocityInvert == false) // if note is smaller than quarter note
                    {
                        for (int i = 0; i < loopAmount; i++)
                        {
                            velocityRequest++;
                            velocity = ((velocityRequest - 1) * velocityAmount) + qNoteVelocity;
                            note.SetVelocity(velocity);
                        }

                    }
                    else if (noteLength > qNoteLength & velocityInvert == false) // if note is larger than quarter note
                    {
                        for (int i = 0; i < largeLoopAmount; i++)
                        {
                            velocityRequest--;
                            velocity = ((velocityRequest + 1) * velocityAmount) + qNoteVelocity;
                            note.SetVelocity(velocity);
                        }
                    }

                    Console.WriteLine("regionNumber=[" + regionNumber + "]");
                    Console.WriteLine("noteLyric=[" + note.GetLyric() + "]");
                    Console.WriteLine("velocityRequest=" + velocityRequest);
                    Console.WriteLine("velocity=" + velocity);
                    Console.WriteLine("noteTempo=" + noteTempo);
                    Console.WriteLine("noteLength=" + noteLength);
                    Console.WriteLine("tempoDifference=" + tempoDifference);
                    Console.WriteLine("velocityInvert=" + velocityInvert);
                    Console.WriteLine("qNoteVelocity=" + qNoteVelocity + "\n");

                    System.Threading.Thread.Sleep(25);

                }

                foreach (Note note in utauPlugin.note) // NON CV
                {
                    // variables once Again omg
                    string tempNoteNum = note.GetNum();
                    string noteNum = null;

                    if (tempNoteNum.All(char.IsDigit) == true)
                    {
                        noteNum = note.GetNum();
                    }
                    else
                    {
                        continue;
                    }

                    int previousVelocity = note.Prev.GetVelocity();

                    // regex to define CV notes
                    Regex findCV = new Regex("[" + consonantScope + "](.*)[" + vowelScope + "](.*)");
                    bool isCV = findCV.IsMatch(note.GetLyric());

                    // regex to define ending notes
                    Regex findEnding = new Regex("(.*)-");
                    bool isEnding = findEnding.IsMatch(note.GetLyric());

                    if (isEnding == true & endingVelocity == false)
                    {
                        note.SetVelocity(note.GetVelocity());
                    }
                    else if (isCV == false)
                    {
                        note.SetVelocity(previousVelocity);
                    }
                    else
                    {
                        continue;
                    }

                    if (modSmooth == true)
                    {
                        note.SetMod(0);
                    }

                    int velocity = note.GetVelocity();

                    Console.WriteLine("regionNumber=[" + noteNum + "]");
                    Console.WriteLine("noteLyric=[" + note.GetLyric() + "]");
                    Console.WriteLine("velocity=" + velocity);
                    Console.WriteLine("previousVelocity=" + previousVelocity + "\n");
                    System.Threading.Thread.Sleep(25);
                }
            }

            if (velocityMode == 4) // DEBUG
            {
                foreach(Note note in utauPlugin.note)
                {
                    string tempNoteNum = note.GetNum();
                    string noteNum = null;

                    if (tempNoteNum.All(char.IsDigit) == true)
                    {
                        noteNum = note.GetNum();
                    }
                    else
                    {
                        continue;
                    }

                    int previousVelocity = note.Prev.GetVelocity();

                    Console.WriteLine(noteNum);
                    Console.WriteLine(previousVelocity);
                    System.Threading.Thread.Sleep(25);
                }
            }

            if (velocityMode > 4)
            {
                Console.WriteLine("Invalid mode!");
            }
            utauPlugin.Output();
        }
    }
}
