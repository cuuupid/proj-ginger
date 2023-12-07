using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	public class KeyboardDispatcher
	{
		protected string _enteredText;

		protected List<char> _commandInputs = new List<char>();

		protected List<Keys> _keysDown = new List<Keys>();

		protected List<char> _charsEntered = new List<char>();

		protected GameWindow _window;

		private IKeyboardSubscriber _subscriber;

		private string _pasteResult = "";

		public IKeyboardSubscriber Subscriber
		{
			get
			{
				return _subscriber;
			}
			set
			{
				if (_subscriber != value)
				{
					if (_subscriber != null)
					{
						_subscriber.Selected = false;
					}
					_subscriber = value;
					if (_subscriber != null)
					{
						_subscriber.Selected = true;
					}
				}
			}
		}

		public void Cleanup()
		{
			_window = null;
		}

		public KeyboardDispatcher(GameWindow window)
		{
			_commandInputs = new List<char>();
			_keysDown = new List<Keys>();
			_charsEntered = new List<char>();
			_window = window;
		}

		public bool ShouldSuppress()
		{
			return false;
		}

		public void Discard()
		{
			_enteredText = null;
			_charsEntered.Clear();
			_commandInputs.Clear();
			_keysDown.Clear();
		}

		public void Poll()
		{
			if (_enteredText != null)
			{
				if (_subscriber != null && !ShouldSuppress())
				{
					_subscriber.RecieveTextInput(_enteredText);
				}
				_enteredText = null;
			}
			if (_charsEntered.Count > 0)
			{
				if (_subscriber != null && !ShouldSuppress())
				{
					foreach (char item in _charsEntered)
					{
						_subscriber.RecieveTextInput(item);
						if (_subscriber == null)
						{
							break;
						}
					}
				}
				_charsEntered.Clear();
			}
			if (_commandInputs.Count > 0)
			{
				if (_subscriber != null && !ShouldSuppress())
				{
					foreach (char commandInput in _commandInputs)
					{
						_subscriber.RecieveCommandInput(commandInput);
						if (_subscriber == null)
						{
							break;
						}
					}
				}
				_commandInputs.Clear();
			}
			if (_keysDown.Count <= 0)
			{
				return;
			}
			if (_subscriber != null && !ShouldSuppress())
			{
				foreach (Keys item2 in _keysDown)
				{
					_subscriber.RecieveSpecialInput(item2);
					if (_subscriber == null)
					{
						break;
					}
				}
			}
			_keysDown.Clear();
		}

		[STAThread]
		private void PasteThread()
		{
			_pasteResult = "";
		}
	}
}
