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
            demo.clpex();
            //demo.TS();  
        }
    }

    public class my_solver
    {
        const int N = 4;//set of n coils waiting to be annealed
        const int F = 3;//set of all availiable annealing furnaces
        const int H = 15;//Height of the inner cover of a furnace
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

            for (int i = 0; i < N; i++)
            {
                gi[i] = random.Next(1, Weight_MAX);
                height_i[i] = random.Next(Height_MAX);
                hi[i] = height_i[i] + H_convector;
                fi[i] = random.Next(1, PRI_MAX);
                Fi[i] = p * fi[i] + (1 - p) * gi[i];
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

            for (int i = 0; i < cost1.Length; i++)
            {
                for (int j = 0; j < cost1[i].Length; j++)
                {
                    Console.WriteLine("cost1_ij[{0}][{1}]={2}", i, j, cost1[i][j]);
                }
            }
            Console.WriteLine();

            for (int i = 0; i < cost2.Length; i++)
            {
                for (int j = 0; j < cost2[i].Length; j++)
                {
                    Console.WriteLine("cost2_ik[{0}][{1}]={2}", i, j, cost2[i][j]);
                }
            }
            Console.WriteLine();

            for (int len = 0; len < Fi.Length; len++)
            {
                Console.WriteLine("Fi[{0}]={1}", len, Fi[len]);

            }
        }

        public void clpex()
        {
            Cplex model = new Cplex();
            //variables
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

                for (int k = 0; k < N/*Rk[i].Length*/; k++)
                {
                   
                    y_ikj[i][k] = new INumVar[F];
                    for (int j = 0; j < F/*Qi[i].Length*/; j++)
                    {
                        y_ikj[i][k][j/*Qi[i][j]*/] = model.NumVar(0, 1, NumVarType.Bool);
                    }
                }
            }

            //obj
            #region
            if (1 == 1)
            {
                INumExpr[] obj = new INumExpr[3];
                obj[0] = model.NumExpr();
                obj[1] = model.NumExpr();
                obj[2] = model.NumExpr();
                for (int j = 0; j < F; j++)
                {
                    for (int i = 0; i < Oj[j].Length; i++)
                    {

                        if (Oj[j][i] == 1)
                        {
                            obj[0] = model.Sum(obj[0], model.Prod(Fi[i], x_ij[i][j]));
                            obj[1] = model.Sum(obj[1], model.Prod(cost1[i][j], x_ij[i][j]));

                        }
                        //obj[0] = model.Sum(obj[0], model.Prod(Fi[i], x_ij[i][j]));
                        //obj[1] = model.Sum(obj[1], model.Prod(cost1[i][j], x_ij[i][j]));

                        for (int k = 0; k < N; k++)
                        {
                            if (Oj[j][k] == 1 && Rk[i][k] == 1)
                            {
                                //obj[2] = model.Sum(obj[0], model.Prod(cost2[i][k], y_ikj[i][k][j]));
                            }
                        }

                    }
                }
                model.AddMaximize(model.Diff(obj[0], obj[1]));
                //model.AddMaximize(model.Diff(obj[0], model.Sum(obj[1], obj[2])));

            }
            #endregion
            //st
            //1
            if (0 == 0)
            {
                INumExpr[] expr1 = new INumExpr[10];
                for (int i = 0; i < N; i++)
                {
                    expr1[0] = model.NumExpr();
                    for (int j = 0; j < F; j++)
                    {
                        if (Qi[i][j] == 1)//j属于Qi
                        {
                            expr1[0] = model.Sum(expr1[0], x_ij[i][j]);
                        }
                    }
                    model.AddLe(expr1[0], 1);
                }
            }
            //2
            if (0 == 0)
            {
                Console.WriteLine("---2---");
                INumExpr[] expr2 = new INumExpr[10];
                for (int j = 0; j < F; j++)
                {
                    expr2[0] = model.NumExpr();
                    for (int i = 0; i < Oj[j].Length; i++)
                    {
                        if (Oj[j][i] == 1)
                        {
                            Console.WriteLine("hi[{0}]={1}", i, hi[i]);
                            expr2[0] = model.Sum(expr2[0], model.Prod(hi[i], x_ij[i][j]));
                        }
                    }
                    model.AddLe(expr2[0], H);
                }
            }
            //3
            if (0 == 0)
            {
                INumExpr[] expr3 = new INumExpr[10];
                for (int j = 0; j < F; j++)
                {
                    expr3[0] = model.NumExpr();
                    for (int k = 0; k < N; k++)
                    {
                        if ((Oj[j][k]) == 1)
                        {
                            //Console.WriteLine("Oj[{0}][{1}]={2}", j, k, Oj[j][k]);
                            expr3[0] = model.Sum(expr3[0], y_ikj[k][k][j]);
                        }
                    }
                    model.AddEq(expr3[0], 1);
                }
                //model.AddEq(expr3[0], 1);//注意下逻辑啊啊啊
            }
            //4
            if (0 == 0)
            {
                for (int j = 0; j < F; ++j)
                {
                    for (int k = 0; k < N; k++)
                    {
                        if ((Oj[j][k]) == 1)
                        {
                            for (int i = 0; i < N; i++)
                            {
                                if (Oj[j][i] == 1 && Rk[k][i] == 1)
                                {
                                    model.AddLe(y_ikj[i][k][j], y_ikj[k][k][j]);
                                }
                            }
                        }
                    }
                }
                /* for (int j = 0; j < F; j++)//j属于P
                 {
                     for (int k0 = 0; k0 < Oj[j].Length; k0++)//k属于Oj ,Oj[i][j]为coil的index                       
                     {
                         int k = Oj[j][k0];
                         for (int i0 = 0; i0 < Rk.Length; i0++)
                         {
                             for (int i1 = 0; i1 < Rk[i1].Length; i1++)//Rk[i][j]为coil的index
                             {
                                 int i = Rk[i0][i1];
                                 if (i == k)//如果i属于Oj∪Rk
                                 {
                                     model.AddLe(y_ikj[i][k][j], y_ikj[k][k][j]);
                                 }
                             }
                         }
                     }

                 }*/
            }
            //5
            if (0 == 0)
            {
                INumExpr[] expr5 = new INumExpr[6];
                for (int j = 0; j < F; j++)//j属于P
                {
                    for (int i = 0; i < N; i++)
                    {
                        if (Oj[j][i] == 1)
                        {
                            expr5[0] = model.NumExpr();
                            for (int k = 0; k < N; k++)
                            {
                                if (Oj[j][k] == 1 && Rk[i][k] == 1)
                                {
                                    expr5[0] = model.Sum(expr5[0], y_ikj[i][k][j]);
                                }
                            }                            
                        }                        
                    }                    
                }
            }

            if (model.Solve())
            {
                Console.WriteLine("ok");
                Console.WriteLine("obj={0}", model.ObjValue);
                //Console.WriteLine("obj[0]_reward={0}", model.GetValue(obj[0]));


                //x_ij
                for (int j = 0; j < F; j++)
                {
                    for (int i = 0; i < N; i++)
                    {
                        try
                        {
                            if (model.GetValue(x_ij[i][j]) > 0)
                            {
                                Console.WriteLine("x_ij[{0}][{1}]={2}",
                                    i, j, model.GetValue(x_ij[i][j]));
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
            //首先给出的是解的情况，第一位是median,
            //第二位开始是在满足高度条件下跟media一起的
            //solution[0][1]=5;表示在0号furnace中，其media为5
            int[][] solution = new int[F][];
            for(int i=0;i<F; i++)
            {
                solution[i]= new int[N];
            }
            //initial solution
            #region
            //1先选一个Oj_size最小的
            //选择最大的reward的i放在里面
            //在满足高度要求的情况下，按照reward的次序依次添加ci
            int[] Oj_size = new int[F];
            for(int i = 0; i < F; i++)
            {
                int count = 0;
                for(int j = 0; j < Oj[i].Length; j++)
                {
                    if (Oj[i][j] == 1)
                    {
                        count++;
                    }
                    Oj_size[i] = count;                   
                }
            }
            int sum_height = 0;
            while (sum_height<H) {                
            }
            //找到furnace中能匹配的最少的
            int minIndex=Array.IndexOf(Oj_size, Oj_size.Min());
            //找到该furnace中最reward最大的
            //Qi里面i和j是相对的
            int size = Oj_size[minIndex];//furnace最小的能装多少

            double[] Fi_Oj=new double[size];

            for(int i=0;i< size; i++)
            {
                Fi_Oj[i] = Fi[Oj[minIndex][i]];
                Console.WriteLine("Fi_Oj[{0}]={1}",i, Fi[Oj[minIndex][i]]);
            }
                        
            int maxIndex=Array.IndexOf(Fi_Oj,Fi_Oj.Max());
            Console.WriteLine("maxIndex=" + maxIndex);

                   
            #endregion

        }
    }
}

