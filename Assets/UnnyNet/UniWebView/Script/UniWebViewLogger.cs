/// <summary>
/// A leveled logger which could log UniWebView related messages in 
/// both development environment and final product.
/// </summary>
public class UniWebViewLogger {
    /// <summary>
    /// Logger level.
    /// </summary>
    public enum Level {
        /// <summary>
        /// Off level. When set to `Off`, the logger will log out nothing.
        /// </summary>
        Off = 0x00000000,

        /// <summary>
        /// Lowest level. When set to `Verbose`, the logger will log out all messages.
        /// </summary>
        Verbose = 0x00000002,

        /// <summary>
        /// Debug level. When set to `Debug`, the logger will log out most of messages up to this level.
        /// </summary>
        Debug = 0x00000003,

        /// <summary>
        /// Info level. When set to `Info`, the logger will log out up to info messages.
        /// </summary>
        Info = 0x00000004,

        Warn = 0x00000005,

        Error = 0x00000006
    }

    private static UniWebViewLogger instance;
    private Level level;
    
    /// <summary>
    /// Current level of this logger. All messages above current level will be logged out.
    /// Default is `Critical`, which means the logger only prints errors and exceptions.
    /// </summary>
    public Level LogLevel {
        get { return level; }
        set {
            level = value;
            UniWebViewInterface.SetLogLevel((int)value);
        }
    }

    private UniWebViewLogger(Level level) {
        this.level = level;
    }

    /// <summary>
    /// Instance of the UniWebView logger across the process. Normally you should use this for logging purpose
    /// in UniWebView, instead of creating a new logger yourself.
    /// </summary>
    public static UniWebViewLogger Instance {
        get {
            if (instance == null) {
                instance = new UniWebViewLogger(Level.Error);
            }
            return instance;
        }
    }

    /// <summary>
    /// Log a verbose message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Verbose(string message) { Log(Level.Verbose, message); }

    /// <summary>
    /// Log a debug message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Debug(string message) { Log(Level.Debug, message); }

    /// <summary>
    /// Log an info message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Info(string message) { Log(Level.Info, message); }

    /// <summary>
    /// Log a critical message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Critical(string message) { Log(Level.Error, message); }

    private void Log(Level lvl, string message) {
        if (lvl >= LogLevel && LogLevel != Level.Off) {
            var logMessage = "<UniWebView> " + message;
            switch (lvl) {
                    case Level.Error:
                    UnityEngine.Debug.LogError(logMessage);
                    break;
                case Level.Warn:
                    UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case Level.Info:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                default:
                    UnityEngine.Debug.Log(logMessage);
                    break;
            }
        }
    }
}