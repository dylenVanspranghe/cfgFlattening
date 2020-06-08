using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
//if (k + l == 59 && j + 7 == 57)

/*
  else
        {
            Console.WriteLine(""Inside else."");
        } 
 */

namespace modifySourceCodeRoslyn
{
    class Program
    {
        static Dictionary<string, string> strings = new Dictionary<string, string>();
        static int cases = 0;
        static int caseID = 0;
        static void Main(string[] args)
        {
            InitializeStrings();
            var Tree = CSharpSyntaxTree.ParseText(@"
using System;

namespace CFGTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if(avg >= 10)
            {
                Console.WriteLine(""Student succeeded. Points: "" + avg);
            } else
            {
                Console.WriteLine(""Student failed. Points: "" + avg);
            }

}
    }
}

");
            
            


            var root = Tree.GetRoot();
            List<MethodDeclarationSyntax> method = new List<MethodDeclarationSyntax>();
            List<String> newMethods = new List<String>();
            List<List<String>> flattenedMethods = new List<List<String>>();
            List<String> result = new List<String>();
            foreach (MethodDeclarationSyntax mds in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                method.Add(mds);
            }

            method.ForEach(mds => newMethods.Add(SplitMethod(mds)));
            newMethods.ForEach(newMethod => {
                flattenedMethods.Add(switchStructure(newMethod));
            });
            flattenedMethods.ForEach(flattenedMethod => result.Add(createMethodSwitch(flattenedMethod)));
            Console.ReadLine();
        }

        private static string createMethodSwitch(List<string> lines)
        {
            String program = "";
            Boolean continueWhile = true;
            int i = 0;

            while (continueWhile)
            {
                if (lines[i] == strings["start-switch"].Trim())
                {
                    program += "while (swVar != "+ caseID + ")\n{\nswitch (swVar)\n{\n";
                    continueWhile = false;
                } else
                {
                    program += lines[i] + "\n";
                }
                i++;
            }

            continueWhile = true;

            while (continueWhile)
            {
                String line = lines[i];
                if (lines[i].Length > 2 && lines[i].Substring(0, 2) == "//")
                {
                    if (lines[i].Contains("case "))
                    {
                        if (!lines[i].Contains("case 0:"))
                        {
                            program += "break;\n";
                        }

                        String caseStatement = lines[i].Split(" | ")[1];
                        program += caseStatement + "\n";
                    }
                    if (lines[i] == strings["end"])
                    {
                        program += "swVar = " + caseID + ";\nbreak;\n}\n}\n";
                    }
                    if (lines[i] == strings["c-if"].Trim() || lines[i] == strings["beic-if"].Trim() || lines[i] == strings["ie-if"].Trim())
                    {
                        if (lines[i] == strings["c-if"].Trim())
                        {
                            program += "if (" + lines[i+1] + ")\n{\n" + lines[i+2] + "\n} ";
                            i = i + 2;
                        } else if (lines[i] == strings["beic-if"].Trim())
                        {
                            program += "else if (" + lines[i + 1] + ")\n{\n" + lines[i + 2] + "\n} ";
                            i = i + 2;
                        } else if (lines[i] == strings["ie-if"].Trim())
                        {
                            program += "else\n{\n" + lines[i+1] + "\n} ";
                            i = i + 1;
                        }

                        if (lines[i+1] != strings["c-if"].Trim() && lines[i+1] != strings["beic-if"].Trim() && lines[i+1] != strings["ie-if"].Trim())
                        {
                            program += "\n";
                        }
                    }
                    
                } else
                {
                    program += lines[i] + "\n";
                }

                i++;
                if (i >= lines.Count)
                {
                    continueWhile = false;
                }
            }







            Console.WriteLine(program);
            return null;
        }

        static List<String> switchStructure(String text)
        {
            // Initialize list of statements, this is the list that will be the result in the end
            string[] textArray = Regex.Split(text, "\n");
            List<String> lines = new List<String>();
            foreach (String line in textArray)
            {
                lines.Add(line);
            }
            Boolean continueWhile = true;
            int i = 0;
            // Go over all the statements and check if it is a instruction.
            //for (int i = 0; i < lines.Count; i++) // change to while and check if there are lines left? since the lines.count will probably change
            while (continueWhile)
            {
                
                    
                if (lines[i] == strings["start-switch"].Trim())
                {
                    lines.Insert(i, "int swVar = " + caseID + ";");
                    lines.Insert(i + 2, strings["case"] + caseID + ":");
                    i = i + 2;
                    caseID++;
                    continueWhile = false;
                }
                     
                
                i++;
            }

            continueWhile = true;

            while (continueWhile)
            {
                if (lines.Count > i)
                {
                    // check if the line is 
                    if (lines[i].Length > 2)
                    {
                        if (lines[i].Substring(0, 2) == "//")
                        {
                            // statement is a instruction
                            if (lines[i] == strings["b-if"].Trim())
                            {
                                // instruction signilazes the beginning of an instruction
                                List<Object> returnvalues = switchStructureIfStatement(lines, i);
                                lines = (List<String>)returnvalues[0];
                                i = int.Parse(returnvalues[1].ToString());
                                lines.Insert(i, strings["case"] + caseID + ":");
                                caseID++;
                                i++;

                                //TODO: bug bij nested if without else the else at the end of the not nested if is not translated

                            }
                        }
                    }
                } else
                {
                    continueWhile = false;
                }
                i++;
            }

            i = 0;
            continueWhile = true;

            while (continueWhile)
            {
                if (lines[i] == strings["b-if"].Trim() || lines[i] == strings["end-if"].Trim() + " | done" || lines[i] == strings["ebl-if"].Trim() || lines[i] == strings["eei-if"].Trim() || lines[i] == strings["ee-if"].Trim() || lines[i] == "")
                {
                    lines.RemoveAt(i);
                    i--;
                }
                i++;
                if (i >= lines.Count)
                {
                    continueWhile = false;
                }
            }

            lines.Insert(i, strings["end"]);

            //lines.ForEach(line => Console.WriteLine(line));

            return lines;
        } //if (lines[i].Length > 2)

        static List<Object> switchStructureIfStatement(List<String> lines, int lineNumber)
        {
            
            int linesAddedAboveBeginInstruction = 0;
            String flag = "";
            int i = lineNumber+1;
            Boolean continueWhile = true;
            
            while (continueWhile)
            {
                if (lines[i].Length > 2 && lines[i].Substring(0, 2) == "//")
                {
                    if (lines[i] == strings["c-if"].Trim() || lines[i] == strings["beic-if"].Trim())
                    {
                        flag = "start condition";
                        lines = InsertLine(lines, i, lineNumber + linesAddedAboveBeginInstruction);
                        linesAddedAboveBeginInstruction++;
                    }
                    else if (lines[i] == strings["ec-if"].Trim() || lines[i] == strings["eeic-if"].Trim())
                    {
                        lines.RemoveAt(i);
                        i--;
                        flag = "end condition";
                    }
                    else if (lines[i] == strings["bl-if"].Trim() || lines[i] == strings["bei-if"].Trim())
                    {
                        lines[i] = lines[i].Split("\n")[0] + " | case " + caseID + ":";
                        caseID++;
                        flag = "start block";
                    } else if (lines[i] == strings["e-if"].Trim())
                    {
                        lines[i] = lines[i].Split("\n")[0] + " | case " + caseID + ":";
                        
                        lines.Insert(lineNumber + linesAddedAboveBeginInstruction, strings["ie-if"]);
                        i++;
                        flag = "start block";
                        linesAddedAboveBeginInstruction++;
                        lines.Insert(lineNumber + linesAddedAboveBeginInstruction, "swVar = " + caseID + ";");
                        linesAddedAboveBeginInstruction++;
                        // i++?
                        i++;
                        caseID++;
                    }
                    else if (lines[i] == strings["ebl-if"].Trim() || lines[i] == strings["ee-if"].Trim() || lines[i] == strings["eei-if"].Trim())
                    {
                        flag = "end block";
                    }
                    else if (lines[i] == strings["end-if"].Trim())
                    {
                        int j = lineNumber;
                        Boolean continueWhile2 = true;
                        while (continueWhile2)
                        {
                            if (lines[j] == strings["ebl-if"].Trim() || lines[j] == strings["ee-if"].Trim() || lines[j] == strings["eei-if"].Trim())
                            {
                                if (lines[j - 1].Split(" ")[0] != "swVar")
                                {
                                    lines.Insert(j, "swVar = " + caseID + ";");
                                    j++;
                                    i++;
                                }
                            } else if (lines[j] == strings["end-if"].Trim())
                            {
                                if (lines[j] != strings["end-if"].Trim() + " | done")
                                {
                                    continueWhile2 = false;
                                    lines[j] = lines[j] + " | done";
                                }
                            }
                            j++;
                        }
                        continueWhile = false;
                        flag = "end if";
                    }
                    else if (lines[i] == strings["b-if"].Trim())
                    {

                        List<Object> returnvaluesnestedif = switchStructureIfStatement(lines, i);

                        lines = (List<String>)returnvaluesnestedif[0];
                        i = (int)returnvaluesnestedif[1];
                        lines.Insert(i, strings["case"] + caseID + ":");
                        caseID++;
                        i++;

                    }
                } else
                {
                    if (flag == "start condition")
                    {
                        lines = InsertLine(lines, i, lineNumber + linesAddedAboveBeginInstruction);
                        linesAddedAboveBeginInstruction++;
                        lines.Insert(lineNumber + linesAddedAboveBeginInstruction, "swVar = " + caseID + ";");
                        i++;
                        linesAddedAboveBeginInstruction++;
                    }
                }
                i++;
            }
            List<Object> returnvalues = new List<Object>();
            returnvalues.Add(lines);
            returnvalues.Add(i);
            return returnvalues;
        }

        public static List<String> InsertLine(List<String> lines, int lineNumberInsert, int lineNumberTarget)
        {
            lines.Insert(lineNumberTarget, lines[lineNumberInsert]);
            lines.RemoveAt(lineNumberInsert + 1);
            return lines;
        }

        static void InitializeStrings()
        {
            strings.Add("b-if", "// Beginning if statement\n");
            strings.Add("end-if", "// End if statement\n");
            strings.Add("c-if", "// Condition if statement\n");
            strings.Add("ec-if", "// End condition if statement\n");
            strings.Add("bl-if", "// Block if statement\n");
            strings.Add("ebl-if", "// End block if statement\n");
            strings.Add("e-if", "// Beginning else statement\n");
            strings.Add("ee-if", "// End else statement\n");
            strings.Add("beic-if", "// Beginning else if condition statement\n");
            strings.Add("bei-if", "// Beginning else if statement\n");
            strings.Add("eeic-if", "// End else if condition statement\n");
            strings.Add("eei-if", "// End else if statement\n");
            strings.Add("ie-if", "// Include else statement");
            strings.Add("start-switch", "// Start switch statement\n");
            strings.Add("end", "// End method\n");
            strings.Add("case", "// new case | case ");
            //strings.Add("", "");
        }

        static String SplitMethod(MethodDeclarationSyntax mds)
        {
            //List<SyntaxNode> original = new List<SyntaxNode>();
            //List<SyntaxNode> newNodes = new List<SyntaxNode>();
            String program = "";
            String variableDeclarations = "";

            //DescendantNodes
            foreach (StatementSyntax blockstatement in mds.ChildNodes().OfType<StatementSyntax>())
            {
                List<Object> results = ReadBlock(blockstatement);
                program += results[0];
                variableDeclarations += results[1];
            }
            program = variableDeclarations + strings["start-switch"] + program;
            return RemoveWhiteSpaces(program);
        }

        static public List<Object> ReadBlock(StatementSyntax blockstatement)
        {//debug
            String program = "";
            String variableDeclarations = "";
            List<SyntaxNode> original = new List<SyntaxNode>();

            foreach (StatementSyntax statement in blockstatement.ChildNodes().OfType<StatementSyntax>())
            {
                if (statement.GetType().Name == "IfStatementSyntax")
                {
                    program += ReadIfStatement(statement);
                    program = program + strings["end-if"];
                }
                else
                {
                    if (statement.GetType().Name == "LocalDeclarationStatementSyntax")
                    {
                        if (statement.ToFullString().Contains("="))
                        {
                            if (statement.ToFullString().Split("=")[0].Split(" ").Where(e => !string.IsNullOrEmpty(e)).ToArray()[0] == "String" || statement.ToFullString().Split("=")[0].Split(" ").Where(e => !string.IsNullOrEmpty(e)).ToArray()[0] == "string")
                            {
                                variableDeclarations += statement.ToFullString().Split("=")[0] + "= new " + statement.ToFullString().Split("=")[0].Split(" ").Where(e => !string.IsNullOrEmpty(e)).ToArray()[0] + "(\"\");\n";
                            } else
                            {
                                variableDeclarations += statement.ToFullString().Split("=")[0] + "= new " + statement.ToFullString().Split("=")[0].Split(" ").Where(e => !string.IsNullOrEmpty(e)).ToArray()[0] + "();\n";
                            }
                            program += statement.ToFullString().Split("=")[0].Split(" ").Where(e => !string.IsNullOrEmpty(e)).ToArray()[statement.ToFullString().Split("=")[0].Split(" ").Where(e => !string.IsNullOrEmpty(e)).ToArray().Count() - 1] + " =" + statement.ToFullString().Split("=")[1]; //foutje als er een string aanwezig is met =
                        } else
                        {
                            variableDeclarations += statement.ToFullString();
                        }

                    } else if (!original.Contains(statement.Parent))
                    {
                        program = program + statement.ToFullString();
                    }
                }
            }
            List<Object> results = new List<Object>();
            results.Add(program);
            results.Add(variableDeclarations);
            return results;
        }

        static public String ReadIfStatement(StatementSyntax statement, Boolean recursive = false)
        {
            String program = "";

            String block = ReadIfStatementBlock((IfStatementSyntax)statement);
            String elseStatements = checkElseStatements((IfStatementSyntax)statement);
            String condition = ReadIfStatementCondition((IfStatementSyntax)statement);
            if (!recursive)
            {
                program = program + strings["b-if"] + strings["c-if"] + condition + "\n" + strings["ec-if"] + strings["bl-if"] + block + "\n" + strings["ebl-if"];
            } else
            {
                program = program + strings["beic-if"]  + condition + "\n" + strings["eeic-if"] + strings["bei-if"] + block + "\n" + strings["eei-if"];
            }
            if (elseStatements != null)
            {
                program = RemoveLines(program, 0, 1);
                program = program + "\n" + elseStatements + "\n";
            }
            return program;
        }

        static public String ReadIfStatementCondition(IfStatementSyntax node)
        {
            var condition = node.Condition;
            return condition.ToFullString();
        }

        static public String ReadIfStatementBlock(IfStatementSyntax node)
        {
            var body = node.Statement;
            return RemoveLines((String)ReadBlock(body)[0], 0, 1);
        }

        static public String checkElseStatements(IfStatementSyntax node)
        {
            String program = "";

            if (node.Else == null)
            {
                return null;
            }
            ElseClauseSyntax newNode = node.Else;
            int nodes = newNode.DescendantNodes().OfType<StatementSyntax>().Count();
            if (nodes > 0 && newNode.DescendantNodes().OfType<StatementSyntax>().ElementAt(0).GetType().Name == "IfStatementSyntax")
            {
                String programPart = "";
                IfStatementSyntax ifstatement = (IfStatementSyntax)newNode.DescendantNodes().OfType<StatementSyntax>().ElementAt(0);
                programPart += ReadIfStatement(ifstatement, true);
                program += programPart;
            } else
            {
                String programPart = "";
                StatementSyntax body = newNode.Statement;
                programPart = program + strings["e-if"] + RemoveLines((String)ReadBlock(body)[0], 0, 1) + "\n" + strings["ee-if"];
                program += programPart;
            }
            return RemoveLines(program, 0, 1);
        }

        // Depricated
        static public String ReadIfStatementElse(IfStatementSyntax node)
        {
            if (node.Else != null)
            {
                String body = node.Else.Statement.ToFullString().Trim();
                return RemoveLines(body, 1, 1);
            }
            return null;
        }

        public static string RemoveLines(string text, int skipBeginning, int skipEnd)
        {
            var lines = Regex.Split(text, "\r\n|\r|\n").Skip(skipBeginning).SkipLast(skipEnd);
            return string.Join(Environment.NewLine, lines.ToArray());
        }

        public static string RemoveWhiteSpaces(String text)
        {
            String newText = "";
            var lines = Regex.Split(text, "\n");
            foreach (String line in lines)
            {
                newText += line.Trim() + "\n";
            }
            return newText;
        }

        public static String CFGFlattening(String text)
        {
            String program = InitializeSwitch();
            var lines = Regex.Split(text, "\n");
            List<String> statements = new List<String>();
            cases = 0;
            int lineNumber = 0;

            //Console.WriteLine(lines[lineNumber]);
            //------------------------------------------------------------------------------------------------------------------------------------------------------------
            //while afstellen op laatste case?
            //naar volgende case op ieder einde van een if statement case// done
            // gedeclareerd veriabelen buiten de switch zetten
            //------------------------------------------------------------------------------------------------------------------------------------------------------------
            int beginStatements = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                
                if (lines[i].Length > 2)
                {
                    if (lines[i].Substring(0, 2) == "//")
                    {
                        
                        if (lines[i] == strings["b-if"].Trim())
                        {
                            List<Object> tempStorage = GetIfCases(lines, i, cases, statements);
                            String ifcases = tempStorage[0].ToString();
                            i = int.Parse(tempStorage[1].ToString());
                            program += ifcases;
                            //Console.WriteLine("we've got an if statement on our hands");
                            /*List<Object> conditionsAndStatements = GetIfCases(lines, lineNumber, cases, cases); // niet nodig
                            List<String> conditions = (List<String>)conditionsAndStatements[0]; // bestaat daar al
                            List<String> conditionCases = new List<String>(); //CaseCounters voor if cases in listform
                            List<List<String>> Stmts = (List<List<String>>)conditionsAndStatements[1]; // bestaat daar al
                            List<String> newCases = new List<string>(); // ifCases
                            int oldCases = cases; // normale case => dinges met my case
                            Stmts.ForEach(statement => {
                                cases++;
                                conditionCases.Add(cases.ToString()); // add new case numbers for if cases
                            });
                            cases++; //set cases on unused number
                            int counter = 0;
                            Stmts.ForEach(statement => {
                                statement.Add("swVar = " + cases + ";");
                                newCases.Add(CreateCase(int.Parse(conditionCases[counter]), statement));
                                counter++;
                                }); //add variable swVar to if cases
                            String baseStatementCase = CreateCase(oldCases, statements, conditions, conditionCases); //create original case (the one that makes the decision)
                            newCases.ForEach(ifpart => baseStatementCase += "\n" + ifpart); // add cases to OG case
                            program += baseStatementCase; // add to program*/
                            beginStatements++;
                            statements = new List<String>();
                        } else if (lines[i] == strings["end-if"].Trim())
                        {
                            beginStatements--;
                        }

                    } else if (beginStatements == 0)
                    {
                        statements.Add(lines[i]);
                    }
                }
                lineNumber++;
            }
            if (statements.Any())
            {
                program += "\n" + CreateCase(cases, statements);
            }
            program += "\n}\n}";
            //program = "";
            return program;
        }

        public static int Linenumberendif(String[] lines, int startLine)
        {
            int lineNumber = startLine;

            for (int i = lineNumber + 1; i < lines.Length; i++)
            {
                if (lines[i] == strings["end-if"].Trim())
                {
                    return i;
                }
            }

            return -1;
        }

        public static List<Object> GetIfCases(String[] lines, int lineNumber, int myCase, List<String> originalStatement, Boolean recursion = false)
        {
            List<String> conditions = new List<String>();
            List<List<String>> statements = new List<List<String>>();
            List<String> conditionCases = new List<String>(); //new
            List<String> ifCases = new List<string>(); //new
            String baseStatementCase = ""; //new
            List<String> storage = new List<String>();
            List<String> nestedIfs = new List<String>();
            List<String> linesBaseStatement = new List<string>();
            List<List<String>> addLater = new List<List<String>>();
            Boolean recursionHappened = false;
            String flag = "";
            // e-if
            for (int i = lineNumber+1; i < lines.Length; i++)
            {
                if (lines[i] == strings["c-if"].Trim() || lines[i] == strings["beic-if"].Trim())
                {
                    flag = "start condition";
                } else if (lines[i] == strings["ec-if"].Trim() || lines[i] == strings["eeic-if"].Trim())
                {
                    flag = "end condition";
                } else if (lines[i] == strings["bl-if"].Trim() || lines[i] == strings["e-if"].Trim() || lines[i] == strings["bei-if"].Trim())
                {
                    flag = "start block";
                }
                else if (lines[i] == strings["ebl-if"].Trim() || lines[i] == strings["ee-if"].Trim() || lines[i] == strings["eei-if"].Trim())
                {
                    flag = "end block";
                } else if (lines[i] == strings["end-if"].Trim())
                {
                    flag = "end if";
                } else if (lines[i] == strings["b-if"].Trim())
                {
                    int counter = 0;
                    statements.ForEach(statement => {
                        counter++;
                    });

                    List<Object> tempStorage = GetIfCases(lines, i, cases, storage, true);
                    nestedIfs.Add(tempStorage[0].ToString());
                    statements.Add((List<String>)tempStorage[2]);
                    storage = new List<String>();
                    //storage.Add("blablabla = " + (cases + counter) + ";");
                    //Console.WriteLine(cases);
                    myCase = cases;
                    //Console.WriteLine(myCase);
                    //Console.WriteLine(i);
                    //Console.WriteLine(Linenumberendif(lines, i));
                    //Console.WriteLine(lines[21]);
                    //Console.WriteLine(tempStorage[1].ToString());
                    i = Linenumberendif(lines, i);
                    recursionHappened = true;
                    
                }

                if (flag == "start condition")
                {
                    if (lines[i].Substring(0, 2) != "//")
                    {
                        storage.Add(lines[i]);
                    }
                } else if (flag == "end condition")
                {
                    storage.ForEach(line => conditions.Add(line));
                    storage = new List<String>();
                } else if (flag == "start block")
                {
                    if (lines[i].Substring(0, 2) != "//")
                    {
                        storage.Add(lines[i]);
                    }
                } else if (flag == "end block")
                {
                    if (recursionHappened)
                    {
                        addLater.Add(storage);
                    } else
                    {
                        statements.Add(storage);
                        storage = new List<String>();
                    }
                    recursionHappened = false;
                } else if (flag == "end if")
                {
                    i = lines.Length;
                    //new
                    statements.ForEach(statement => {
                        cases++;
                        conditionCases.Add(cases.ToString()); // add new case numbers for if cases
                    });
                    cases++; //set cases on unused number
                    int counter = 0;
                    statements.ForEach(statement => {
                        statement.Add("swVar = " + cases + ";");
                        ifCases.Add(CreateCase(int.Parse(conditionCases[counter]), statement));
                        counter++;
                    }); //add variable swVar to if cases
                    baseStatementCase = CreateCase(myCase, originalStatement, conditions, conditionCases); //create original case (the one that makes the decision)
                    foreach (String line in Regex.Split(RemoveLines(baseStatementCase, 1, 0), "\n"))
                    {
                        linesBaseStatement.Add(line);
                    }
                        
                    
                    ifCases.ForEach(ifpart => baseStatementCase += "\n" + ifpart); // add cases to OG case
                    lineNumber = i;
                }
            }
            nestedIfs.ForEach(nestdcase => baseStatementCase += "\n" + nestdcase);

            
            List<Object> returnvalue = new List<Object>();
            returnvalue.Add(baseStatementCase);
            returnvalue.Add(lineNumber);
            returnvalue.Add(linesBaseStatement);
            //Console.WriteLine("-----------------------9-------------------------");
            //Console.WriteLine(baseStatementCase);
            //Console.WriteLine("-----------------------9-------------------------");
            return returnvalue;
        }

        public static String CreateCase(int cases, List<String> statements, List<String> conditions = null, List<String> conditionCases = null)
        {
            String offset = "\n\t\t";
            String caseStatement = "\tcase " + cases + ":";
            statements.ForEach(statement => caseStatement += offset + statement);
            if (conditions != null)
            {
                caseStatement += offset + CreateIfStatement(conditions, conditionCases);
            }
            caseStatement += offset + "break;";

            //Console.WriteLine(caseStatement);
            return caseStatement;
        }

        public static String CreateIfStatement(List<String> conditions, List<String> conditionCases)
        {
            String ifstatement = "if (" + conditions[0] + ")\n{\n\tswVar = " + conditionCases[0] + ";\n} ";
            if (conditionCases.Count == 1) { return ifstatement; }

            for (int i = 1; i < conditions.Count; i++)
            {
                ifstatement += "else if (" + conditions[i] + ")\n{\n\tswVar = " + conditionCases[i] + ";\n} ";
            }

            if (conditions.Count < conditionCases.Count)
            {
                ifstatement += "else \n{\n\tswVar = "+ conditionCases[conditionCases.Count-1] + ";\n}";
            }
            //Console.WriteLine(ifstatement);
            return ifstatement;
        }

        public static String InitializeSwitch()
        {
            String switchStatement = @"
int swVar = 0;
while (swVar != -1)
{
    switch (swVar)
    {
";

            //int swVar = 1;
            //while (swVar != -1)
            //{
                //switch (swVar)
                //{
                    //case 1:
                        //Console.WriteLine("Case 1");
                        //swVar++;
                        //break;
                    //case 2:
                        //Console.WriteLine("Case 2");
                        //swVar = -1;
                        //break;
                    //default:
                        //Console.WriteLine("Default case");
                        //break;
                //}
            //}
            

            return switchStatement;
        }


        public static List<String> GenerateStringArray(Int32 index)
        {
            List<String> result = new List<String>();

            for (int i = 0; i < index; i++)
            {
                result.Add("Test"+i);
            }

            return result;
        }

    }

    public class SampleChanger : CSharpSyntaxRewriter
    {
        public SyntaxNode VisitIfStatement(IfStatementSyntax node)
        {
            return node;
            // Generates a node containing only parenthesis 
            // with no identifier, no return type and no parameters
            var newNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName(""), "");
            // Removes the parenthesis from the Parameter List 
            // and replaces them with MissingTokens
            newNode = newNode.ReplaceNode(newNode.ParameterList,
                newNode.ParameterList.WithOpenParenToken(
                    SyntaxFactory.MissingToken(SyntaxKind.OpenParenToken)).
                WithCloseParenToken(SyntaxFactory.MissingToken(SyntaxKind.CloseParenToken)));
            // Returns the new method containing no content 
            // but the Leading and Trailing trivia of the previous node
            return newNode.WithLeadingTrivia(node.GetLeadingTrivia()).
                WithTrailingTrivia(node.GetTrailingTrivia());
        }
    }
}
