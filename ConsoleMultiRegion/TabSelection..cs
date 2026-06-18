using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMultiRegion
{
    class TabSelection
    {
        public static void Run(string[] args)
        {
            // 调用自定义的选择方法
            bool isConfirmed = ShowConfirmation("是否要格式化 C 盘？");

            Console.Clear();
            if (isConfirmed)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("正在执行格式化操作...");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("操作已取消。");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 自定义交互式确认框
        /// </summary>
        static bool ShowConfirmation(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("(使用 Tab/方向键 切换，Enter 确认，Y/N 快捷选择)");

            int selectedIndex = 0; // 0: 是, 1: 否
            int startY = Console.CursorTop; // 记录选项所在行的 Y 坐标，用于反复重绘

            while (true)
            {
                // 1. 移动光标到选项行，并清空该行（防止残留字符）
                Console.SetCursorPosition(0, startY);
                Console.Write(new string(' ', Console.WindowWidth - 1));
                Console.SetCursorPosition(0, startY);

                // 2. 绘制 "[ 是 ]"
                if (selectedIndex == 0) SetHighlightStyle();
                Console.Write("[ 是 ]");
                Console.ResetColor();

                Console.Write("    "); // 选项间距

                // 3. 绘制 "[ 否 ]"
                if (selectedIndex == 1) SetHighlightStyle();
                Console.Write("[ 否 ]");
                Console.ResetColor();

                // 4. 监听键盘输入 (true 表示不在屏幕上打印按下的键)
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                // 5. 处理交互逻辑
                if (keyInfo.Key == ConsoleKey.LeftArrow || (keyInfo.Key == ConsoleKey.Tab && keyInfo.Modifiers == ConsoleModifiers.Shift))
                {
                    selectedIndex--;
                    if (selectedIndex < 0) selectedIndex = 1;
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow || keyInfo.Key == ConsoleKey.Tab)
                {
                    selectedIndex++;
                    if (selectedIndex > 1) selectedIndex = 0;
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine("\n");
                    return selectedIndex == 0;
                }
                // 快捷键支持
                else if (keyInfo.Key == ConsoleKey.Y) 
                {
                    Console.WriteLine("\n"); 
                    return true;
                }
                else if (keyInfo.Key == ConsoleKey.N) 
                { 
                    Console.WriteLine("\n"); 
                    return false; 
                }
            }
        }

        // 设置高亮样式（黑底白字）
        static void SetHighlightStyle()
        {
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
        }
    }
}
