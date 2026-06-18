using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleMultiRegion
{
    class MultiRegion
    {
        private static readonly object _consoleLock = new object();
        private static bool _isRunning = true;

        private static readonly Queue<LogEntry> _logQueue = new Queue<LogEntry>();
        private static int _logStartY;
        private static int _logEndY;
        private static int _maxLogLines;
        private static int _maxLogWidth;

        public static void Run(string[] args)
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; 
                _isRunning = false;
            };

            // 1. 初始化控制台
            Console.CursorVisible = false;
            Console.Title = "多区域动态控制台演示";
            Console.Clear();

            // 2. 绘制静态边框/布局
            DrawLayout();

            // 3. 启动多个后台任务
            Task.Run(() => UpdateRegion_Clock());
            Task.Run(() => UpdateRegion_Progress());
            Task.Run(() => UpdateRegion_Logs());

            // 4. 提示退出
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("按任意键或 Ctrl+C 退出程序...");
            Console.ResetColor();

            while (_isRunning)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    _isRunning = false;
                    break;
                }
                Thread.Sleep(100);
            }

            // 5. 统一执行清理工作，确保无论怎么退出都不会乱套
            CleanupConsole();
        }

        /// <summary>
        /// 清理控制台状态，恢复原貌
        /// </summary>
        static void CleanupConsole()
        {
            // 给后台线程一点时间响应 _isRunning = false 并退出循环
            Thread.Sleep(200);

            // 恢复控制台默认状态
            Console.ResetColor();
            Console.CursorVisible = true;
            Console.Clear();

            // 将光标移动到安全位置并输出提示
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("程序已优雅退出。");
        }

        /// <summary>
        /// 绘制初始布局边框
        /// </summary>
        static void DrawLayout()
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;
            int midX = width / 2;
            int midY = height / 2;

            for (int y = 0; y < midY; y++) SafeWrite(midX, y, "│");
            for (int x = 0; x < width - 1; x++) SafeWrite(x, midY, "─");

            SafeWrite(2, 0, "[ 系统时间 ]");
            SafeWrite(midX + 2, 0, "[ 任务进度 ]");
            SafeWrite(2, midY + 1, "[ 运行日志 (滚动) ]");
        }

        static void UpdateRegion_Clock()
        {
            while (_isRunning)
            {
                SafeWrite(2, 2, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Thread.Sleep(1000);
            }
        }

        static void UpdateRegion_Progress()
        {
            int progress = 0;
            int midX = Console.WindowWidth / 2;

            while (_isRunning)
            {
                progress = (progress + 1) % 101;
                int barWidth = 20;
                int filled = (int)(barWidth * (progress / 100.0));
                string bar = "[" + new string('█', filled) + new string(' ', barWidth - filled) + $"] {progress}%";
                SafeWrite(midX + 2, 2, bar);
                Thread.Sleep(50);
            }
        }

        static void UpdateRegion_Logs()
        {
            int midY = Console.WindowHeight / 2;
            _logStartY = midY + 3;
            _logEndY = Console.WindowHeight - 2;
            _maxLogLines = _logEndY - _logStartY + 1;
            _maxLogWidth = Console.WindowWidth - 4;

            string[] levels = { "INFO", "WARN", "ERROR", "DEBUG" };
            Random rnd = new Random();

            while (_isRunning)
            {
                string level = levels[rnd.Next(levels.Length)];
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                string msg = $"{time} [{level}] 执行任务 {Guid.NewGuid().ToString().Substring(0, 8)} 完成";
                ConsoleColor color = GetLogLevelColor(level);

                AddLogAndRedraw(msg, color);
                Thread.Sleep(rnd.Next(100, 600));
            }
        }

        static void AddLogAndRedraw(string newLog, ConsoleColor color)
        {
            if (!_isRunning) return;

            lock (_consoleLock)
            {
                if (!_isRunning) return;

                if (newLog.Length > _maxLogWidth)
                    newLog = newLog.Substring(0, _maxLogWidth);
                else
                    newLog = newLog.PadRight(_maxLogWidth);

                _logQueue.Enqueue(new LogEntry { Text = newLog, Color = color });

                while (_logQueue.Count > _maxLogLines) _logQueue.Dequeue();

                int currentY = _logStartY;
                string emptyLine = new string(' ', _maxLogWidth);

                for (int y = _logStartY; y <= _logEndY; y++)
                {
                    if (y < Console.WindowHeight && 2 < Console.WindowWidth)
                    {
                        Console.SetCursorPosition(2, y);
                        Console.Write(emptyLine);
                    }
                }

                foreach (var log in _logQueue)
                {
                    if (currentY < Console.WindowHeight && 2 < Console.WindowWidth)
                    {
                        Console.SetCursorPosition(2, currentY);
                        Console.ForegroundColor = log.Color;
                        Console.Write(log.Text);
                        currentY++;
                    }
                }
                Console.ResetColor();
            }
        }

        static ConsoleColor GetLogLevelColor(string level)
        {
            switch (level)
            {
                case "ERROR": return ConsoleColor.Red;
                case "WARN": return ConsoleColor.Yellow;
                case "DEBUG": return ConsoleColor.DarkGray;
                default: return ConsoleColor.Green;
            }
        }

        static void SafeWrite(int x, int y, string text)
        {
            if (!_isRunning) return;

            lock (_consoleLock)
            {
                if (!_isRunning) return;

                if (x >= 0 && x < Console.WindowWidth && y >= 0 && y < Console.WindowHeight)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(text);
                }
            }
        }

        class LogEntry
        {
            public string? Text { get; set; }
            public ConsoleColor Color { get; set; }
        }
    }
}