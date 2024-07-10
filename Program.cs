using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    public static void Main()
    {
        const int N = 2048;
        var Nantou = GiveNantou(N);
        Console.WriteLine($"710 = {RetrieveTree(Nantou, 710)}");
    }
    public static Expression[,] GiveNantou(int N)
    {
        // 0: 7-7
        // 1: 7-10
        // 2: 10-7
        // 3: 10-10
        Expression[,] Nantou = new Expression[N, 4];
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Nantou[i, j] = new Expression(Operator.Atom, 0, 0, false);
            }
        }
        //[i,j]で式の形がどこに移るか
        var Composition = new int[4, 4] {
            {-1, -1, 0, 1},
            {0, 1, -1, -1},
            {-1, -1, 2, 3},
            {2, 3, -1, -1},
        };
        Nantou[7, 0] = new Expression(true);
        Nantou[10, 3] = new Expression(false);
        var Updated = new Queue<(int k, int t)>();
        Updated.Enqueue((7, 0));
        Updated.Enqueue((10, 3));

        while (Updated.Any())
        {
            (int k, int type) = Updated.Dequeue();
            for (int other = 0; other < N; other++)
            {
                // どの式形式と結合するか
                for (int typeL = 0; typeL < 4; typeL++)
                {
                    var new_type = Composition[typeL, type];
                    if (new_type < 0) continue;
                    // どの演算子で結合するか
                    for (int oper = 1; oper <= 4; oper++)
                    {
                        if (oper == 4 && (k == 0 || other % k != 0)) continue;
                        var answer = oper switch
                        {
                            1 => other + k,
                            2 => Math.Abs(other - k),
                            3 => other * k,
                            4 => other / k,
                            _ => other
                        };
                        if (answer < 0 || answer >= N) continue;
                        if (Nantou[answer, new_type].len > Nantou[k, type].len + Nantou[other, typeL].len)
                        {
                            Nantou[answer, new_type] = new Expression((Operator)oper, other, k, type < 2, Nantou[k, type].len + Nantou[other, typeL].len);
                            Updated.Enqueue((answer, new_type));
                        }
                    }
                }
                for (int typeR = 0; typeR < 4; typeR++)
                {
                    var new_type = Composition[type, typeR];
                    if (new_type < 0) continue;
                    // どの演算子で結合するか
                    for (int oper = 1; oper <= 4; oper++)
                    {
                        if (oper == 4 && (other == 0 || k % other != 0)) continue;
                        var answer = oper switch
                        {
                            1 => k + other,
                            2 => Math.Abs(k - other),
                            3 => k * other,
                            4 => k / other,
                            _ => other
                        };
                        if (answer < 0 || answer >= N) continue;
                        if (Nantou[answer, new_type].len > Nantou[k, type].len + Nantou[other, typeR].len)
                        {
                            Nantou[answer, new_type] = new Expression((Operator)oper, k, other, typeR < 2, Nantou[k, type].len + Nantou[other, typeR].len);
                            Updated.Enqueue((answer, new_type));
                        }
                    }
                }
            }
        }
        return Nantou;
    }

    /// <summary>
    /// 答えがNになる強南東式を取得する
    /// </summary>
    /// <param name="N"></param>
    public static string RetrieveTree(Expression[,] Nantou, int N)
    {
        // 式木を取得
        // var Formula = new List<string>();
        var formula = RetrieveTree(Nantou, N, 1, out _, out _);
        return formula;
    }

    private static string RetrieveTree(Expression[,] Nantou, int N, int type, out int priority, out int subpriority)
    {
        static string Represent(Operator oper)
        {
            var res = oper switch
            {
                Operator.Plus => "+",
                Operator.Minus => "-",
                Operator.Times => "×",
                Operator.Div => "/",
                _ => "",
            };
            return res;
        }
        var E = Nantou[N, type];
        var oper = E.oper;
        if (oper == Operator.Atom)
        {
            priority = -1;
            subpriority = 1;
            return E.A.ToString();
        }

        priority = oper switch
        {
            Operator.Plus => 2,
            Operator.Minus => 2,
            Operator.Times => 1,
            Operator.Div => 1,
            _ => 0,
        };
        subpriority = oper switch
        {
            Operator.Plus => 1,
            Operator.Minus => 0,
            Operator.Times => 1,
            Operator.Div => 0,
            _ => 0,
        };
        var A = E.A;
        var B = E.B;
        bool flag = oper == Operator.Minus && A < B;
        var operSymbol = Represent(oper);
        var r7 = E.isRightFromSeven;
        /*
            0 -- 7 | 1t0 | 0f2
            1 -- 0f3 | 1t1
            2 -- 3t0 | 2f2
            3 -- 10 | 3t1 | 2f3
        */
        var trueDst = new int[4, 2] { { 1, 0 }, { 1, 1 }, { 3, 0 }, { 3, 1 } };
        var falseDst = new int[4, 2] { { 0, 2 }, { 0, 3 }, { 2, 2 }, { 2, 3 } };
        var typeA = r7 ? trueDst[type, 0] : falseDst[type, 0];
        var typeB = r7 ? trueDst[type, 1] : falseDst[type, 1];
        var left = RetrieveTree(Nantou, A, typeA, out var leftPrior, out var leftSubprior);
        var right = RetrieveTree(Nantou, B, typeB, out var rightPrior, out var rightSubprior);

        // if (leftPrior < priority)
        var leftDelParen = leftPrior < 0 || leftPrior <= priority;
        if (!leftDelParen)
            left = $"({left})";
        // if (rightPrior > priority || (rightPrior == priority && rightSubprior > subpriority))
        var rightDelParen = rightPrior < 0 || rightPrior < priority || rightPrior == priority && rightSubprior < subpriority;
        if (!rightDelParen)
            right = $"({right})";
        var res = $"{left}{operSymbol}{right}";
        if (flag)
        {
            priority = -1;
            return $"|{res}|";
        }
        else return res;
    }
}