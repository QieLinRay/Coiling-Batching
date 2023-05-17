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

namespace Coiling_Batching
{
    internal class Program
    {
        static void Main(string[] args)
        {
            my_solver demo=new my_solver();
            demo.data();
        }
    }


    /*public void data()
    {
        #region
        Random random = new Random(1);
        const int N = 20;//set of n coils waiting to be annealed
        const int F = 5;//set of all availiable annealing furnaces
        const int H = 15;//Height of the inner cover of a furnace
        const int Weight_MAX = 3;
        const int Height_MAX = 5;
        const int PRI_MAX = 10;
        const int H_convector = 1;
        const int C1_MAX = 8;
        const int C2_MAX = 5;
        int[] gi= new int[N];//weight of coil i;
        int[] height_i = new int[N]; 
        int[] hi= new int[N];//sum of width?? of coil i
                             //and the height of a convector plate

        int[] fi= new int[N];//PRI value og coil i
        double[]Fi= new double[N];//rewarding for covering i
                                  //in the batching plan
        const double p = 0.5;
        int[][] cost1= new int[N][];//Mismatching cost between coil i and furnace j
        int[][] cost2= new int[N][];//mismatching cost between coil i and coil k;
        //int[]O_fi = new int[F];
        //Set of coils that can be loaded into furnace j
        //following the equipment and matching
        int[][]Rk = new int[N][];
        //set of coils that can be loaded together with coil k
        //following the matching constraints
        int[]Qi=new int[N];//availiable annealing furnaces
                           //where coil i can be loaded

        for(int k=0;k<N; k++) 
        { 
            int num=random.Next(N-1);//随机生成可以和k一批的数量
            for(int n = 0; n < num; n++)
            {
                int temp = random.Next(N);
                if (temp != k)
                {
                    Rk[k][n] = 1;
                }
            }                      
        }


        for(int i = 0; i < N; i++)
        {
            Qi[i] = random.Next(F);
            gi[i] = random.Next(Weight_MAX);
            height_i[i]=random.Next(Height_MAX);
            hi[i] = height_i[i] + H_convector;
            fi[i] = random.Next(PRI_MAX);
            Fi[i] = p * fi[i]+(1-p)*gi[i];
            for(int j=0;j<F;j++) 
            {
                cost1[i][j] = random.Next(C1_MAX);
            }
            for(int k = 0; k < N; k++)
            {
                cost2[i][k] = random.Next(C2_MAX);
            }
        }
        #endregion
    }*/

    public class my_solver
    {
        const int N = 10;//set of n coils waiting to be annealed
        const int F = 5;//set of all availiable annealing furnaces
        const int H = 15;//Height of the inner cover of a furnace
        const int Weight_MAX = 3;
        const int Height_MAX = 5;
        const int PRI_MAX = 10;
        const int H_convector = 1;
        const int C1_MAX = 8;
        const int C2_MAX = 5;
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
            #region
            Random random = new Random(1);
            Console.WriteLine("----Oj----");
            int sum = 0;
            for (int j = 0; j < F - 1; j++)
            {
                int temp= random.Next(1, N - sum - (F - j - 1));
                Oj[j] = new int[temp];
                sum += temp;              
            }
            Oj[F - 1] = new int[N - sum];
        


            List<int> numList = new List<int>();
            for (int i = 0; i < N; i++)
            {
                numList.Add(i);
            }

            int count = 0;
            while (count < N)
            {
                     
                for(int j = 0; j < Oj.Length; j++)
                {
                    for(int i = 0; i < Oj[j].Length; i++)
                    {
                        int temp = random.Next(0, N - count);
                        Oj[j][i] = numList[temp];
                        numList.RemoveAt(temp);
                        count++;
                    }                    
                }
            }

            Console.WriteLine("----Oj----");

            for(int j=0;j<Oj.Length; j++)
            {
                for(int i = 0; i < Oj[j].Length; i++)
                {
                    Console.WriteLine("Oj[{0}][{1}]={2}",j,i,Oj[j][i]);
                }
            }



            /*Console.WriteLine("----Oj----");
            int sum = 0;
            for (int j = 0; j < F - 1; j++)
            {
                Oj[j] = random.Next(1, N - sum - (F - j - 1));
                sum += Oj[j];
                Console.WriteLine("O[{0}]={1}", j, Oj[j]);
            }
            Oj[F - 1] = N - sum;
            Console.WriteLine("O[{0}]={1}", F - 1, N - sum);
            Console.WriteLine("----Oj----");*/


           
            for(int k = 0; k < N; k++)
            {
                int len=random.Next(1, N - 1);
                Rk[k]=new int[len];
                for(int n=0;n< len; n++)
                {
                    List<int> num2 = new List<int>();
                    int temp = random.Next(N);
                    if (temp != k && !num2.Contains(temp))
                    {
                        num2.Add(temp);
                        Rk[k][n] = temp;
                    }
                }
            }


           /* for (int k = 0; k < N; k++)
            {               
                int num = random.Next(N - 1);//随机生成可以和k一批的数量
                Rk[k] = new int[num];
                for (int n = 0; n < num; n++)
                {
                    int temp = random.Next(N);
                    if (temp != k)
                    {
                        Rk[k][n] = temp;//可以和k一起的coil的index
                    }
                }
            }*/
            for(int k = 0; k < Rk.Length; k++)
            {
                for(int n = 0;n < Rk[k].Length; n++)
                {
                    Console.WriteLine("Rk[{0}][{1}]={2}", k, n, Rk[k][n]);
                }
            }


            /*List<int> numList = new List<int>();
            for (int i = 0; i < N; i++)
            {
                numList.Add(i);                
            }*/

            /*int count = 0;
            while (count < N)
            {
                for(int j = 0; j < F; j++)
                {
                    Qi[j] = new int[Oj[j]]; 
                    for(int i = 0; i < Oj[j]; i++)
                    {
                        int temp = random.Next(0, N - count);
                        Qi[j][i] = numList[temp];
                        numList.RemoveAt(temp);
                        count++;
                    }
                }

            }*/

            for(int i=0;i< N; i++)
            {
                int temp = random.Next(1, F);//一个coil对应的furnace的数量
                Qi[i] = new int[temp];
                for(int j = 0; j < temp; j++)
                {
                    int furnace=random.Next(F);
                    Qi[i][j] = furnace;//第i个coil对应的第j个可以用的furnace
                    //Console.WriteLine("Qi[{0}][{1}]={2}", i, j, Qi[i][j]);
                }
            }

            
            for(int j = 0; j < F; j++)
            {
                for(int i = 0; i < Qi[j].Length; i++)
                {
                    //Console.WriteLine("Qi[{0}][{1}]={2}", j, i, Qi[j][i]);
                }
            }

            if (1 == 2)
            {
                for (int j = 0; j < F; j++)//Oj[]the avaliable number in furnace j
                {
                    Console.WriteLine("----j----");
                    /*Qi[j] = new int[Oj[j]];
                    for (int i = 0; i < Oj[j]; i++)
                    {
                        //Console.WriteLine("i={0}", i);
                        int temp = random.Next(0, N - j * i);
                        Qi[j][i] = temp;
                        Console.WriteLine("Q[{0}][{1}]={2}", j, i, Qi[j][i]);
                        numList.Remove(temp);
                    }*/
                    Console.WriteLine("----j----");
                }

                for (int j = 0; j < F; j++)
                {
                    for (int i = 0; i < Qi[j].Length; i++)
                    {
                        Console.WriteLine("Q[{0}][{1}]={2}", j, i, Qi[j][i]);
                    }
                }
            }

            for (int i = 0; i < N; i++)
            {
               
                gi[i] = random.Next(Weight_MAX);
                height_i[i] = random.Next(Height_MAX);
                hi[i] = height_i[i] + H_convector;
                fi[i] = random.Next(PRI_MAX);
                Fi[i] = p * fi[i] + (1 - p) * gi[i];

                cost1[i] = new int[F];
                cost2[i] = new int[N];
                for (int j = 0; j < F; j++)
                {
                    
                    cost1[i][j] = random.Next(C1_MAX);
                }
                for (int k = 0; k < N; k++)
                {
                    cost2[i][k] = random.Next(C2_MAX);
                }               
            }
            #endregion
        }

        public void clpex()
        {
            
            Cplex model = new Cplex();
            //variables
            INumVar[][] x_ij = new INumVar[N][];
            INumVar[][][] y_ikj = new INumVar[N][][];
         
            for (int i = 0; i < N; i++)
            {
                x_ij[i] = new INumVar[Qi[i].Length];
                for (int j = 0; j < Qi[i].Length; j++)
                {                                   
                    x_ij[i][Qi[i][j]] = model.NumVar(0, 1, NumVarType.Bool);                                      
                }
            }

            for (int i = 0; i < N; i++)
            {
                y_ikj[i] = new INumVar[Rk[i].Length][];
                for(int k = 0; k < Rk[i].Length; k++)
                {
                    y_ikj[i][k] = new INumVar[Qi[i].Length];
                    for (int j = 0; j < Qi[i].Length; j++)
                    {
                        y_ikj[i][k][Qi[i][j]] = model.NumVar(0, 1, NumVarType.Bool);
                    }
                }              
            }

            //obj
            //INumExpr obj = model.NumExpr();
            INumExpr[] obj=new INumExpr[10];
            obj[0] = model.NumExpr();
            obj[1] = model.NumExpr();
            obj[2] = model.NumExpr();
            
            for (int j = 0; j < F; j++)
            {
                for(int i0 = 0; i0 < Oj[j].Length; i0++)
                {
                    int i = Oj[j][i0];
                    obj[0] = model.Sum(obj[0], model.Prod(Fi[i], x_ij[i][j]));
                    obj[1] = model.Sum(obj[1], model.Prod(cost1[i][j], x_ij[i][j]));

                    for(int k0 = 0; k0 < Rk[i].Length; k0++)
                    {
                        int k= Rk[i][k0];
                        if (i == k)
                        {
                            obj[2] = model.Sum(obj[2], model.Prod(cost2[i][k], y_ikj[i][k][j]));

                        }
                    }
                }
            }

            model.AddMaximize(model.Diff(obj[0], model.Sum(obj[0], obj[1])));

            




            //st

            //1
            if (1 == 1)
            {
                INumExpr[] expr1 = new INumExpr[10];
                for (int i = 0; i < N; i++)
                {
                    expr1[0] = model.NumExpr();
                    for (int j = 0; j < Qi[i].Length; j++)
                    {
                        expr1[0] = model.Sum(expr1[0], x_ij[i][Qi[i][j]]);
                    }
                    model.AddLe(expr1[0], 1);
                }
            }
            
            //2
            if (0 == 0)
            {
                INumExpr[] expr2= new INumExpr[10];
                for(int j = 0; j < F; j++)
                {
                    expr2[0] = model.NumExpr();
                    for(int i = 0; i < Oj[j].Length;i++)
                    {
                        expr2[0] = model.Sum(expr2[0], model.Prod(hi[Oj[j][i]], x_ij[Oj[j][i]][j]));
                    }
                }
                model.AddLe(expr2[0], H);

            }
            //3
            if (3 == 3)
            {
                INumExpr[] expr3 = new INumExpr[10];
                for(int j=0; j < F; j++)
                {
                    expr3[0] = model.NumExpr();
                    for(int k = 0; k < Oj[j].Length; k++)
                    {
                        expr3[0] = model.Sum(expr3[0], y_ikj[Oj[j][k]][Oj[j][k]][j]);
                    }
                }
                model.AddEq(expr3[0], 1);   
            }

            //4
            if (4 == 4)
            {
                for (int j = 0; j < F; j++)//j属于P
                {
                    for (int k0 = 0; k0 < Oj[j].Length; k0++)//k属于Oj ,Oj[i][j]为coil的index                       
                    {
                        int k = Oj[j][k0];
                        for (int i0 = 0; i0 < Rk.Length; i0++)
                        {
                            for(int i1=0;i1< Rk[i1].Length; i1++)//Rk[i][j]为coil的index
                            {
                                int i = Rk[i0][i1];
                                if (i== k)//如果i属于Oj∪Rk
                                {                                   
                                    model.AddLe(y_ikj[i][k][j], y_ikj[k][k][j]);
                                }
                            }
                        }
                    }

                }

            }

            //5
            if (5 == 5)
            {
                INumExpr[] expr5=new INumExpr[6];
                for(int j = 0; j < F; j++)//j属于P
                {
                    for(int i0 = 0; i0 < Oj[j].Length; i0++)
                    {
                        int i = Oj[j][i0];//i属于Oj
                        expr5[0] = model.NumExpr();                       
                        for(int k0=0; k0 < Rk[i].Length; k0++)
                        {
                            int k = Rk[i0][k0];
                            if (i == k)//k属于Oj∪Ri
                            {
                                expr5[0] = model.Sum(expr5[0], y_ikj[i][k][j]);
                            }
                        }
                        model.AddEq(x_ij[i][j], expr5[0]);
                    }
                }

            }

           
        }
    }
}

