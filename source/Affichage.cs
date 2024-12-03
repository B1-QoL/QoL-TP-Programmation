using System.Collections;
using System.Runtime.CompilerServices;

namespace B1;

/// <summary>
/// Fonctions d'affichage.
/// </summary>
public static class Affichage
{
    ///<summary>
    /// Affiche dans la console un objet de n'importe quel type. Si l'objet est une collection ou un tuple, la fonction affiche tous les éléments imbriqués suivant cette règle :
    /// <list type="bullet">
    /// <item>
    /// tuple => () ;
    /// </item>
    /// <item>
    /// éléments d'un dictionnaire => [] ;
    /// </item>
    /// <item>
    /// pour tout le reste => {}.
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="obj">Objet à afficher. Il peut être de n'importe quel type.</param>
    /// <param name="colorBrackets">Colorie les différentes dimension de <c>obj</c> si sa valeur est <c>true</c>.</param>
    /// <param name="endLine">Ajoute ou non un retour à la ligne à la fin de l'affichage.</param>
    /// <typeparam name="TAny">N'importe quel type.</typeparam>
    public static void Print<TAny>(TAny obj, bool colorBrackets = false, bool endLine = true) {
        int color = 0;
        List<ConsoleColor> colorList = colorBrackets ? new List<ConsoleColor> { ConsoleColor.DarkBlue, ConsoleColor.DarkMagenta, ConsoleColor.DarkGreen, ConsoleColor.Magenta} : new List<ConsoleColor> {ConsoleColor.Gray};
    
        void PrintAux<TAny2> (TAny2 obj2)
        {
            switch (obj2)
            {
                case IEnumerable enumerable and not string:
                {
                    Console.ForegroundColor = colorList[color++ % colorList.Count];
                    Console.Write("{ ");

                    IEnumerator enumerator = enumerable.GetEnumerator();
                    using var enumerator1 = enumerator as IDisposable;
                    bool b = false;

                    while (enumerator.MoveNext())
                    {
                        if (b)
                        {
                            Console.ForegroundColor = colorList[--color % colorList.Count];
                            Console.Write(", ");
                            Console.ForegroundColor = colorList[color++ % colorList.Count];
                        }
                        b = true;

                        object? item = enumerator.Current;

                        switch (item)
                        {
                            case not null when item.GetType().IsGenericType && item.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>):
                                Console.ForegroundColor = colorList[color++ % colorList.Count];;
                                Console.Write("[");
                                PrintAux(item.GetType().GetProperty("Key")!.GetValue(item));
                        
                                Console.ForegroundColor = colorList[--color % colorList.Count];
                                Console.Write(", ");
                                Console.ForegroundColor = colorList[color++ % colorList.Count];
                        
                                PrintAux(item.GetType().GetProperty("Value")!.GetValue(item));
                                Console.ForegroundColor = colorList[--color % colorList.Count];
                                Console.Write("]");
                                break;
                            
                            case IEnumerable inCol:
                                PrintAux(inCol);
                                break;
                            
                            default:
                                Console.Write(item);
                                break;
                        }
                    }
                    
                    Console.ForegroundColor = colorList[--color % colorList.Count];
                    Console.Write(" }");
                    break;
                }
                case ITuple obj3:
                {
                    int length = obj3.Length;
                    
                    Console.ForegroundColor = colorList[color++ % colorList.Count];
                    Console.Write("("); ;
                    
                    for (int i = 0; i < length; i++)
                    {
                        PrintAux(obj3[i]);

                        if (i != length - 1)
                        {
                            Console.ForegroundColor = colorList[--color % colorList.Count];
                            Console.Write(", ");
                            Console.ForegroundColor = colorList[color++ % colorList.Count];
                        }
                    }
                    
                    Console.ForegroundColor = colorList[--color % colorList.Count];
                    Console.Write(")");
                    break;
                    
                }
                default:
                    Console.Write(obj2);
                    break;
            }
        }
        
        PrintAux(obj);
        Console.ResetColor();
        
        if (endLine)
            Console.WriteLine();
    }

    /// <summary>
    /// Affiche un retour à la ligne si rien n'est donné en paramètre.
    /// </summary>
    public static void Print(string obj = "")
    {
        Print(obj,colorBrackets:false);
    }

}