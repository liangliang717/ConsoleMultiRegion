using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleMultiRegion
{
    class Program
    {
        static void Main(string[] args)
        {
            // 多区域展示
            //MultiRegion.Run(args);

            // 列表选择
            ListSelection.Run(args);

            // 选项切换
            //TabSelection.Run(args);

            Console.ReadLine();
        }
    }
}