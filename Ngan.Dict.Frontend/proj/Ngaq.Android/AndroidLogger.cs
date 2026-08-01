//global::Android.Util.Log
namespace Ngaq.Android;

using System;
using Microsoft.Extensions.Logging;
using AndroidLog = global::Android.Util.Log;

public class AndroidLogger : ILogger {

	private readonly string _categoryName;
	private readonly Func<string, LogLevel, bool>? _filter;

	public AndroidLogger(string categoryName, Func<string, LogLevel, bool>? filter = null) {
		_categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
		_filter = filter;
	}

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
		// Android Log doesn't have built-in scope support, return a no-op disposable
		return new NoOpDisposable();
	}

	public bool IsEnabled(LogLevel logLevel) {
		// If a custom filter is provided, use it
		if (_filter != null) {
			return _filter(_categoryName, logLevel);
		}

		// Map Microsoft LogLevel to Android LogPriority and check if loggable
		var priority = LogLevelToAndroidPriority(logLevel);
		return AndroidLog.IsLoggable(_categoryName, priority);
	}

	public void Log<TState>(
		LogLevel logLevel
		,EventId eventId
		,TState state
		,Exception? exception
		,Func<TState,Exception?,string> formatter
	) {
		if (!IsEnabled(logLevel)) {
			return;
		}

		if (formatter == null) {
			throw new ArgumentNullException(nameof(formatter));
		}

		var message = formatter(state, exception);
		if (string.IsNullOrEmpty(message)) {
			return;
		}

		var tag = _categoryName;

		try {
			switch (logLevel) {
				case LogLevel.Trace:
				case LogLevel.Debug:
					if (exception != null) {
						AndroidLog.Debug(tag, $"{message}\n{exception}");
					} else {
						AndroidLog.Debug(tag, message);
					}
					break;

				case LogLevel.Information:
					if (exception != null) {
						AndroidLog.Info(tag, $"{message}\n{exception}");
					} else {
						AndroidLog.Info(tag, message);
					}
					break;

				case LogLevel.Warning:
					if (exception != null) {
						AndroidLog.Warn(tag, $"{message}\n{exception}");
					} else {
						AndroidLog.Warn(tag, message);
					}
					break;

				case LogLevel.Error:
				case LogLevel.Critical:
					if (exception != null) {
						AndroidLog.Error(tag, $"{message}\n{exception}");
					} else {
						AndroidLog.Error(tag, message);
					}
					break;

				default:
					// For any other levels, use Info as default
					if (exception != null) {
						AndroidLog.Info(tag, $"{message}\n{exception}");
					} else {
						AndroidLog.Info(tag, message);
					}
					break;
			}
		} catch {
			// Android logging should never throw exceptions
			// If it does, there's nothing we can do but ignore
		}
	}

	private static global::Android.Util.LogPriority LogLevelToAndroidPriority(LogLevel logLevel) {
		return logLevel switch {
			LogLevel.Trace => global::Android.Util.LogPriority.Verbose,
			LogLevel.Debug => global::Android.Util.LogPriority.Debug,
			LogLevel.Information => global::Android.Util.LogPriority.Info,
			LogLevel.Warning => global::Android.Util.LogPriority.Warn,
			LogLevel.Error => global::Android.Util.LogPriority.Error,
			LogLevel.Critical => global::Android.Util.LogPriority.Assert,
			_ => global::Android.Util.LogPriority.Info,
		};
	}

	private class NoOpDisposable : IDisposable {
		public void Dispose() {
			// No-op
		}
	}
}
