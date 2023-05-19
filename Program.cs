using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ILOG.Concert;
using ILOG.Concert;
using ILOG.CPLEX;

namespace Coil_Batching
{
    internal class Program
    {
        static void Main(string[] args)
        {
            my_solver demo = new my_solver();
            demo.data();
            //demo.clpex();
            demo.TS();

        }
    }

    public class my_solver
    {
        const int N = 8;//set of n coils waiting to be annealed
        const int F = 3;//set of all availiable annealing furnaces
        const int H = 10;//Height of the inner cover of a furnace
        const int Weight_MAX = 3;
        const int Height_MAX = 5;
        const int PRI_MAX = 10;
        const int H_convector = 1;
        const int C1_MAX = 3;
        const int C2_MAX = 3;
        int[] gi = new int[N];//weight of coil i;
        int[] height_i = new int[N];
        int[] hi = new int[N];//sum of width?? of coil i
                              //and the height of a convector plate
        int[] fi = new int[N];//PRI value og coil i
        double[] Fi = new double[N];//rewarding for covering i
                                    //in the batching plan
        const double p = 0.5;
        int[][] cost1 = new int[N][];//Mismatching cost between coil i and furnace j
        int[][] cost2 = new int[N][];//mismatching cost between coil i and coil k;
        int[][] Oj = new int[F][];
        //Set of coils that can be loaded into furnace j
        //following the equipment and matching
        int[][] Rk = new int[N][];
        //set of coils that can be loaded together with coil k
        //following the matching constraints
        int[][] Qi = new int[N][];//availiable annealing furnaces
                                  //where coil i can be loaded
                                  //data_generate
        public void data()
        {
            Random random = new Random(1);
            for (int i = 0; i < F; i++)
            {
                Oj[i] = new int[N];
            }
            int[] Oj_size = new int[N];
            for (int i = 0; i < N; i++)
            {
                Oj_size[i] = random.Next(1, F);
            }

            List<int> list_Oj = new List<int>();
            for (int j = 0; j < F; j++)
            {
                list_Oj.Add(j);
            }
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < F; j++)
                {
                    for (int num = 0; num < Oj_size[i]; num++)
                    {
                        int index0 = random.Next(0, F);
                        Oj[list_Oj[index0]][i] = 1;
                    }
                }
            }

            for (int j = 0; j < Oj.Length; j++)
            {
                for (int i = 0; i < Oj[j].Length; i++)
                {
                    if (Oj[j][i] != 0)
                    {
                        Console.WriteLine("Oj[{0}][{1}]={2}", j, i, Oj[j][i]);

                    }
                }
            }
            Console.WriteLine("---Oj---");
            Console.WriteLine();


            Console.WriteLine("---Rk---");
            for (int k = 0; k < N; k++)
            {
                Rk[k] = new int[N];
            }
            List<int> list_Rk = new List<int>();
            for (int i = 0; i < N; i++)
            {
                list_Rk.Add(i);
            }
            int count_Rk = 0;
            while (count_Rk < N)
            {
                Console.WriteLine("count_Rk={0}", count_Rk);
                for (int i = 0; i < N; i++)
                {
                    //Console.WriteLine("N-count_Rk={0}", N - count_Rk);
                    int index = random.Next(0, N - i);
                    if (index != i)
                    {
                        Rk[i][index] = 1;
                        Rk[index][i] = 1;
                        count_Rk++;
                    }
                }
            }

            for (int k = 0; k < Rk.Length; k++)
            {
                for (int n = 0; n < Rk[k].Length; n++)
                {
                    if (Rk[k][n] != 0)
                    {
                        Console.WriteLine("Rk[{0}][{1}]={2}", k, n, Rk[k][n]);
                    }

                }
            }
            Console.WriteLine("---Rk---");
            Console.WriteLine();


            for (int i = 0; i < N; i++)
            {
                Qi[i] = new int[F];
            }

            for (int j = 0; j < F; j++)
            {
                for (int i = 0; i < Oj[j].Length; i++)
                {
                    if (Oj[j][i] == 1)
                    {
                        Qi[i][j] = 1;
                    }
                }
            }

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < Qi[i].Length; j++)
                {
                    if (Qi[i][j] != 0)
                    {
                        Console.WriteLine("Qi[{0}][{1}]={2}", i, j, Qi[i][j]);

                    }
                }
            }
            Console.WriteLine("---Qi---");
            Console.WriteLine();
            Fi[0] = 1; Fi[1] = 2; Fi[2] = 3; Fi[3] = 4;
            Fi[4] = 5; Fi[5] = 6; Fi[6] = 7; Fi[7] = 8;
            for (int i = 0; i < N; i++)
            {
                gi[i] = random.Next(1, Weight_MAX);
                height_i[i] = random.Next(Height_MAX);
                hi[i] = height_i[i] + H_convector;
                fi[i] = random.Next(1, PRI_MAX);
                //Fi[i] = p * fi[i] + (1 - p) * gi[i];
                cost1[i] = new int[F];
                cost2[i] = new int[N];
                for (int j = 0; j < F; j++)
                {

                    cost1[i][j] = random.Next(1, C1_MAX);
                }
                for (int k = 0; k < N; k++)
                {
                    cost2[i][k] = random.Next(1, C2_MAX);
                }
            }
            Console.WriteLine("---hi---");
            for (int i = 0; i < hi.Length; i++)
            {
                Console.WriteLine("hi[{0}]={1}", i, hi[i]);
            }
            Console.WriteLine("---hi---");


            for (int i = 0; i < cost1.Length; i++)
            {
                for (int j = 0; j < cost1[i].Length; j++)
                {
                    //Console.WriteLine("cost1_ij[{0}][{1}]={2}", i, j, cost1[i][j]);
                }
            }
            Console.WriteLine();

            for (int i = 0; i < cost2.Length; i++)
            {
                for (int j = 0; j < cost2[i].Length; j++)
                {
                    //Console.WriteLine("cost2_ik[{0}][{1}]={2}", i, j, cost2[i][j]);
                }
            }
            Console.WriteLine();
            Console.WriteLine("---Fi---");
            for (int len = 0; len < Fi.Length; len++)
            {
                Console.WriteLine("Fi[{0}]={1}", len, Fi[len]);

            }
            Console.WriteLine("---Fi---");
            Console.WriteLine();
        }

        public static int Min(int a, int b)
        {
            return a < b ? a : b;
        }

        public static int[] ToArray(int[] a, int[] b)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < b.Length; j++)
                {
                    if (a[i] == 1 && b[j] == 1 && !list.Contains(i))
                    {
                        list.Add(i);
                    }
                }
            }
            int[] array = list.ToArray();
            return array;
        }

        public static int[] ToArray2(int[] a)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 1)
                {
                    list.Add(i);
                }
            }
            int[] arr = list.ToArray();
            return arr;


        }//将a中等于1的元素的角标存入数组中

        public static bool Contains(int a, int[][] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                for (int j = 0; j < b[i].Length; j++)
                {
                    if (b[i][j] == a)
                    {                      
                        return true;                     
                    }
                }
            }
            return false;           
        }

        public static bool Contain(int a, List<int> b)
        {
            for(int i=0;i<b.Count;i++)
            {
                if (b[i] == a)
                {
                    return true;                
                }               
            }
            return false;
        }

        public static bool isEqual0(double[] b)
        {
            bool flag = true;
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] != 0)
                {
                    flag = false;
                    return flag;
                }
                break;
            }
            return flag;

        }

        public void clpex()
        {
            Cplex model = new Cplex();



            for (int j = 0; j < F; j++)
            {
                for (int k = 0; k < Oj[j].Length; k++)
                {
                    Console.WriteLine("k=" + k);
                    if (Oj[j][k] == 0)
                    {
                        int[] test = ToArray(Oj[j], Rk[k]);
                        for (int len = 0; len < test.Length; len++)
                        {
                            Console.WriteLine("test=" + test[len]);
                        }

                    }
                }
            }
            //variables
            #region
            INumVar[][] x_ij = new INumVar[N][];
            INumVar[][][] y_ikj = new INumVar[N][][];
            for (int i = 0; i < N; i++)
            {
                x_ij[i] = new INumVar[F];
                for (int j = 0; j < F; j++)
                {
                    x_ij[i][j] = model.NumVar(0, 1, NumVarType.Bool);
                }
            }

            for (int i = 0; i < N; i++)
            {
                y_ikj[i] = new INumVar[N][];
                for (int k = 0; k < N; k++)
                {
                    y_ikj[i][k] = new INumVar[F];
                    for (int j = 0; j < F; j++)
                    {
                        y_ikj[i][k][j] = model.NumVar(0, 1, NumVarType.Bool);
                    }
                }
            }

            //obj
            INumExpr[] obj = new INumExpr[N];
            obj[0] = model.NumExpr();
            for (int j = 0; j < F; j++)
            {
                for (int i = 0; i < Oj[j].Length; i++)
                {
                    if (Oj[j][i] == 1)
                    {
                        obj[0] = model.Sum(obj[0], x_ij[i][j]);
                    }
                }
            }
            model.AddMaximize(obj[0]);

            //1
            INumExpr[] expr = new INumExpr[N];
            for (int i = 0; i < N; i++)
            {
                expr[0] = model.NumExpr();
                for (int j = 0; j < Qi[i].Length; j++)
                {
                    if (Qi[i][j] == 1)
                    {
                        expr[0] = model.Sum(expr[0], x_ij[i][j]);
                    }
                }
                model.AddLe(expr[0], 1);
            }

            //2
            INumExpr[] expr2 = new INumExpr[N];
            for (int j = 0; j < F; j++)
            {
                expr2[0] = model.NumExpr();
                for (int i = 0; i < Oj[j].Length; i++)
                {
                    if (Oj[j][i] == 1)
                    {
                        expr2[0] = model.Sum(expr2[0], model.Prod(hi[i], x_ij[i][j]));
                    }
                }
                model.AddLe(expr2[0], H);
            }

            //3
            INumExpr[] expr3 = new INumExpr[N];
            for (int j = 0; j < F; j++)
            {
                expr3[0] = model.NumExpr();
                for (int k = 0; k < Oj[j].Length; k++)
                {
                    if (Oj[j][k] == 1)
                    {
                        expr3[0] = model.Sum(expr3[0], y_ikj[k][k][j]);
                    }
                }
                model.AddEq(expr3[0], 1);
            }

            //4          
            for (int j = 0; j < F; j++)
            {
                for (int k = 0; k < Oj[j].Length; k++)
                {
                    if (Oj[j][k] == 1)
                    {
                        int[] i_array = ToArray(Oj[j], Rk[k]);
                        for (int i = 0; i < i_array.Length; i++)
                        {
                            model.AddLe(y_ikj[i][k][j], y_ikj[k][k][j]);
                        }
                    }
                }
            }

            //5
            INumExpr[] expr5 = new INumExpr[N];
            for (int j = 0; j < F; j++)
            {
                for (int i = 0; i < Oj[j].Length; i++)
                {
                    if (Oj[j][i] == 1)
                    {
                        expr5[0] = model.NumExpr();

                        int[] k_array = ToArray(Oj[j], Rk[i]);
                        for (int k = 0; k < k_array.Length; k++)
                        {
                            expr5[0] = model.Sum(expr5[0], y_ikj[i][k][j]);
                        }
                        model.AddEq(expr5[0], x_ij[i][j]);
                    }
                }
            }
            #endregion

            if (model.Solve())
            {
                Console.WriteLine("ok");
                Console.WriteLine("obj={0}", model.ObjValue);
                //Console.WriteLine("obj[0]_reward={0}", model.GetValue(obj[0]));

                //x_ij
                for (int j = 0; j < F; j++)
                {
                    for (int i = 0; i < x_ij[j].Length; i++)
                    {
                        try
                        {
                            if (model.GetValue(x_ij[j][i]) > 0)
                            {
                                Console.WriteLine("x_ij[{0}][{1}]={2}",
                                    j, i, model.GetValue(x_ij[j][i]));
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                //y_ikj

                for (int j = 0; j < F; j++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        for (int k = 0; k < N; k++)
                        {
                            try
                            {
                                if (model.GetValue(y_ikj[i][k][j]) > 0)
                                {
                                    Console.WriteLine("y_ikj[{0}][{1}][{2}]={3}", i, k, j,
                                        model.GetValue(y_ikj[i][k][j]));
                                }
                            }
                            catch { }

                        }

                    }
                }
            }

            else
            {
                Console.WriteLine("no solution");
            }

        }



        public void TS()
        {
            int[][] b = new int[2][];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = new int[2];
            }
            b[0][0] = 0; b[0][1] = 1; b[1][0] = 0; b[1][1] = 0;
            int a = 9;
            Console.WriteLine("test:" + Contains(a, b));
            //Console.WriteLine("test--0:"+isEqual0(b));
            //首先给出的是解的情况，第一位是median,
            //第二位开始是在满足高度条件下跟media一起的
            //solution[0][1]=5;表示在0号furnace中，其media为5
            //int[][] solution = new int[F][];
            // for (int i = 0; i < F; i++)
            //{
            //   solution[i] = new int[N];
            // }
            int[][] final_solution = new int[F][];

            for (int i = 0; i < F; i++)
            {
                final_solution[i] = new int[N];
                for (int j = 0; j < N; j++)
                {
                    final_solution[i][j] = 9999;

                }
            }

            int[] Oj_size = new int[F];
            for (int i = 0; i < F; i++)//第i个furnace的中可以装coil的数量
            {
                int count = 0;
                for (int j = 0; j < Oj[i].Length; j++)
                {
                    if (Oj[i][j] == 1)
                    {
                        count++;
                    }
                    Oj_size[i] = count;
                }
            }

            for (int j0 = 0; j0 < F; j0++)
            {
                Console.WriteLine("---{0}---", j0);
                #region initial solution
                //1先选一个Oj_size最小的
                //选择最大的reward的i放在里面
                //在满足高度要求的情况下，按照reward的次序依次添加ci              
                //找到furnace中能匹配的最少的
                //int minIndex = Array.IndexOf(Oj_size, Oj_size.Min());
                //找到该furnace中最reward最大的
                //Qi里面i和j是相对的
                //int size = Oj_size[minIndex];//furnace最小的能装多少

                List<int> solution = new List<int>();
                int minIndex = Array.IndexOf(Oj_size, Oj_size.Min());
                Console.WriteLine("minIndex=" + minIndex);
                int Oj_count = Oj_size[minIndex];//furnace最小的能装多少
                Console.WriteLine("Oj_count=" + Oj_count);
                int[] coil_index = ToArray2(Oj[minIndex]);//Oj[minIndex]可以存的coil的角标
                double[] coil_Fi = new double[coil_index.Length];
                for (int i = 0; i < coil_index.Length; i++)
                {
                    coil_Fi[i] = Fi[i];
                    Console.WriteLine("coil_Fi[{0}]={1}", i, coil_Fi[i]);
                }
                for (int i = 0; i < coil_index.Length; i++)
                {

                    Console.WriteLine("hi[{0}]={1}", i, hi[i]);
                }
                #region Oj一维解
                double coil_Max;
                int coil_Max_index = 0;
                int height_sum = 0;
                int count0 = 0;
                Console.WriteLine("H=" + H);
                Console.WriteLine("Before_height_Sum=" + height_sum);
                int temp_index = 999;
                double temp_value = 0;
                while (count0 < Oj_count)
                //如果sum_height小于10，
                //coil_Max_index判断一直进不去，count加不上，就会陷入死循环
                {
                    //是嵌套不是顺序关系
                    /*if (height_sum > H || Fi[coil_Max_index] == 0.0)//后面可能Fi全部赋值为0，就会导致死循环
                    {
                        height_sum -= hi[temp_index];//是上一次加多的
                        coil_Fi[temp_index] = temp_value;//赋值为0了要还回去
                        solution.Remove(temp_index);
                        break;
                    }*/
                    coil_Max = coil_Fi.Max();
                    temp_value = coil_Max;
                    coil_Max_index = Array.IndexOf(coil_Fi, coil_Max);
                    Console.WriteLine("coil_Max_index" + coil_Max_index);                   
                    //height_sum += hi[coil_Max_index];//该点没有加入list,但是高度却加了进去

                    if (!Contains(coil_Max_index, final_solution) 
                        &&!Contain(coil_Max_index,solution))
                    //同一步还没有同步掉final-solution,所以会有两个0
                    {
                        height_sum += hi[coil_Max_index];
                        solution.Add(coil_Max_index);
                        coil_Fi[coil_Max_index] = 0.0;
                        temp_index = coil_Max_index;
                        count0++;
                        if (height_sum > H /*|| Fi[coil_Max_index] == 0.0*/)//后面可能Fi全部赋值为0，就会导致死循环
                        {
                            height_sum -= hi[temp_index];//是上一次加多的
                            coil_Fi[temp_index] = temp_value;//赋值为0了要还回去
                            solution.Remove(temp_index);
                            break;
                        }

                    }
                    else
                    {
                        if (isEqual0(coil_Fi))
                        {
                            break;//不然会一直给下面的赋值为0
                        }
                        else
                        {
                            coil_Fi[coil_Max_index] = 0.0;
                        }                       
                    }

                }
                Console.WriteLine("After_Height=" + height_sum);
                Oj_size[minIndex] = 9999;

                //赋值给最终解
                int[] temp_solution = solution.ToArray();
                for (int i = 0; i < temp_solution.Length; i++)
                {
                    final_solution[minIndex][i] = temp_solution[i];
                }
                /*for(int i=0;i<final_solution.Length;i++)
                {
                    for(int j = 0; j < final_solution[i].Length; j++)
                    {
                        Console.WriteLine("final_solution[{0}][{1}]={2}", i, j, final_solution[i][j]);
                    }
                }*/

                //Oj_size[minIndex] = 9999;


                

                #endregion

                #endregion

               

            }


            for (int j = 0; j < F; j++)
            {
                for (int i = 0; i < N; i++)
                {
                    Console.WriteLine("final_solution[{0}][{1}]={2}", j, i, final_solution[j][i]);
                }
            }

            



            /*#region initial solution
            //1先选一个Oj_size最小的
            //选择最大的reward的i放在里面
            //在满足高度要求的情况下，按照reward的次序依次添加ci
            List<int> solution = new List<int>();
            int[] Oj_size = new int[F];
            for (int i = 0; i < F; i++)//第i个furnace的中可以装coil的数量
            {
                int count = 0;
                for (int j = 0; j < Oj[i].Length; j++)
                {
                    if (Oj[i][j] == 1)
                    {
                        count++;
                    }
                    Oj_size[i] = count;
                }
            }

            for (int i = 0; i < Oj_size.Length; i++)
            {
                Console.WriteLine("Oj_size[{0}]={1}", i, Oj_size[i]);
            }


            //找到furnace中能匹配的最少的
            //int minIndex = Array.IndexOf(Oj_size, Oj_size.Min());
            //找到该furnace中最reward最大的
            //Qi里面i和j是相对的
            //int size = Oj_size[minIndex];//furnace最小的能装多少

            int minIndex = Array.IndexOf(Oj_size, Oj_size.Min());
            int Oj_count = Oj_size[minIndex];//furnace最小的能装多少
            int[] coil_index = ToArray2(Oj[minIndex]);//Oj[minIndex]可以存的coil的角标
            double[] coil_Fi = new double[coil_index.Length];
            for (int i = 0; i < coil_index.Length; i++)
            {
                coil_Fi[i] = Fi[i];
                Console.WriteLine("coil_Fi[{0}]={1}", i, coil_Fi[i]);
            }
            *//*#region 数组
            double[] coil_Max = new double[Oj_count];
            int[] coil_Max_index = new int[Oj_count];
            int height_sum = 0;

            Console.WriteLine("H=" + H);
            while (height_sum < H)
            {
                for (int i = 0; i < Oj_count; i++)
                {
                    coil_Max[i] = coil_Fi.Max();
                    Console.WriteLine("coil_Max="+coil_Max[i]);
                    coil_Max_index[i] = Array.IndexOf(coil_Fi, coil_Max[i]);
                    Console.WriteLine("coil_hi=" + hi[coil_Max_index[i]]);
                                        
                    solution[i] = coil_Max_index[i];
                    coil_Fi[coil_Max_index[i]] = 0.0;
                    height_sum += hi[coil_Max_index[i]];
                    Console.WriteLine("height_sum=" + height_sum);
                }

            }

            #endregion*//*

            #region Oj一维解
            double coil_Max;
            int coil_Max_index;
            int height_sum = 0;
            int count0 = 0;
            Console.WriteLine("H=" + H);
            while (count0<Oj_count)
            {
                coil_Max = coil_Fi.Max();
                //Console.WriteLine("coil_Max=" + coil_Max);
                coil_Max_index = Array.IndexOf(coil_Fi, coil_Max);
                //Console.WriteLine("coil_hi=" + hi[coil_Max_index]);
                height_sum += hi[coil_Max_index];
                //Console.WriteLine("height_sum=" + height_sum);
                if (height_sum > H)
                {
                    height_sum -= hi[coil_Max_index];
                    break;
                }
                solution.Add(coil_Max_index);
                coil_Fi[coil_Max_index] = 0.0;
                count0++;                                  
            }
            for(int i=0;i<solution.Count;i++)
            {
                Console.WriteLine("solution[{0}]={1}",i,solution[i]);
            }
           
            Console.WriteLine("height="+height_sum);
            int[] temp_solution = solution.ToArray();
            //Console.WriteLine(temp_solution.Length);
            for(int i=0;i<temp_solution.Length;i++) {
                Console.WriteLine("temp_solution[{0}]={1}", i, temp_solution[i]);
            }


            #endregion


            #endregion*/

        }
    }
}

