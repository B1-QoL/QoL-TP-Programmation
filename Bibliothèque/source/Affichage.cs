using System.Collections;
using System.Runtime.CompilerServices;

namespace B1;

/// <summary>
/// Fonctions d'affichage.
/// </summary>
public static class Affichage
{
    /// <summary>
    /// Liste des couleurs à utiliser pour les dimensions.
    /// </summary>
    private static List<ConsoleColor> _colors = new ();
    
    private static void PrintAux<T> (T obj, int color)
    {
        switch (obj)
        {
            case null:
                Console.ForegroundColor = _colors[color % _colors.Count];
                Console.Write("null");
                break;
            
            case IEnumerable enumerable and not string:
            {
                ++color;
                Console.ForegroundColor = _colors[color % _colors.Count];
                Console.Write("{ ");

                IEnumerator enumerator = enumerable.GetEnumerator();
                using IDisposable? enumerator1 = enumerator as IDisposable;
                bool b = false;

                while (enumerator.MoveNext())
                {
                    if (b)
                    {
                        Console.ForegroundColor = _colors[color % _colors.Count];
                        Console.Write(", ");
                    }
                    b = true;

                    object? item = enumerator.Current;
                    
                    switch (item)
                    {
                        case null:
                            Console.ForegroundColor = _colors[color % _colors.Count];
                            Console.Write("null");
                            break;
                        
                        case IEnumerable: case ITuple: case object when item.GetType().IsGenericType && 
                                                                        item.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>):
                            PrintAux(item, color);
                            break;
                        
                        default:
                            Console.ForegroundColor = _colors[color % _colors.Count];
                            Console.Write(item);
                            break;
                    }
                }
                
                Console.ForegroundColor = _colors[color % _colors.Count];
                Console.Write(" }");
                --color;
                break;
            }
            case ITuple tuple:
            {
                int length = tuple.Length;
                
                Console.ForegroundColor = _colors[++color % _colors.Count];
                Console.Write("(");
                
                for (int i = 0; i < length; i++)
                {
                    PrintAux(tuple[i], color);

                    if (i != length - 1)
                    {
                        Console.ForegroundColor = _colors[color % _colors.Count];
                        Console.Write(", ");
                    }
                }
                
                Console.ForegroundColor = _colors[color % _colors.Count];
                Console.Write(")");
                --color;
                break;
                
            }
            
            case object when obj.GetType().IsGenericType && 
                    obj.GetType().GetGenericTypeDefinition() == typeof(KeyValuePair<,>):
                Console.ForegroundColor = _colors[++color % _colors.Count];
                Console.Write("[");
                            
                PrintAux(obj.GetType().GetProperty("Key")!.GetValue(obj), color);
                            
                Console.ForegroundColor = _colors[color % _colors.Count];
                Console.Write(", ");
                            
                PrintAux(obj.GetType().GetProperty("Value")!.GetValue(obj), color);
                            
                Console.ForegroundColor = _colors[color % _colors.Count];
                Console.Write("]");
                --color;
                break;
            
            default:
                Console.ForegroundColor = _colors[color % _colors.Count];
                Console.Write(obj);
                break;
        }
    }

    /// <summary>
    ///  Affiche dans la console un objet de n'importe quel type. Si l'objet est une collection ou un tuple, la fonction affiche tous les éléments imbriqués suivant cette règle :
    ///  <list type="bullet">
    ///  <item>
    ///  tuple => () ;
    ///  </item>
    ///  <item>
    ///  éléments d'un dictionnaire => [] ;
    ///  </item>
    ///  <item>
    ///  pour tout le reste => {}.
    ///  </item>
    ///  </list>
    ///  </summary>
    ///  <param name="obj">Objet à afficher. Il peut être de n'importe quel type.</param>
    ///  <param name="colorBrackets">Colorie les différentes dimension de <c>obj</c> si sa valeur est <c>true</c>.</param>
    ///  <param name="endLine">Ajoute ou non un retour à la ligne à la fin de l'affichage.</param>
    /// <param name="newLineAtDim1">Retourne à la ligne entre chaques éléments de la première dimension.</param>
    /// <param name="colors">Liste des couleurs à utiliser pour les dimensions.</param>
    /// <typeparam name="T">N'importe quel type.</typeparam>
    public static void Print<T>(T obj, bool colorBrackets = false, bool endLine = true, bool newLineAtDim1 = false, List<ConsoleColor>? colors = null) 
    {
        if (colorBrackets)
        {
            _colors = colors ?? new List<ConsoleColor> 
            { 
                ConsoleColor.DarkBlue, 
                ConsoleColor.DarkMagenta, 
                ConsoleColor.DarkGreen,
                ConsoleColor.Yellow,
                ConsoleColor.Magenta,
                ConsoleColor.Cyan
            };
        }
        else
        {
            _colors = new List<ConsoleColor> {Console.ForegroundColor};
        }

        if (newLineAtDim1 && obj is IEnumerable enumerable)
        {
            Console.ForegroundColor = _colors[0];
            Console.WriteLine("{");
            
            IEnumerator enumerator = enumerable.GetEnumerator();
            using IDisposable? enumerator1 = enumerator as IDisposable;
            bool start = true;
            
            while (enumerator.MoveNext())
            {
                if (start)
                    start = false;

                else
                {
                    Console.ForegroundColor = _colors[1];
                    Console.WriteLine(",");
                }
                
                Console.Write("     ");
                PrintAux(enumerator.Current,1);
            }
            
            Console.ForegroundColor = _colors[0];
            Console.Write("\n}");
        }
        else
        {
            PrintAux(obj, -1);
        }
        
        if (endLine)
            Console.WriteLine();
        
        Console.ResetColor();
    }
    
    /// <summary>
    /// Si aucun argument n'est donné en paramètre, un retour à la ligne est affiché.
    /// </summary>
    public static void Print()
    {
        Console.WriteLine();
    }
}
