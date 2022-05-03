//-----------------------------------------
//			Advanced Input Field
// Copyright (c) 2017 Jeroen van Pienbroek
//------------------------------------------

using UnityEngine;

namespace AdvancedInputFieldPlugin
{
	/// <summary>Class to format text whenever the text of caret position changes</summary>
	public abstract class LiveProcessingFilter: MonoBehaviour
	{
		/// <summary>Processes a change in raw text (when needed). Can be used to intercept a change</summary>
		/// <param name="textEditFrame">The new text edit frame</param>
		/// <param name="lastTextEditFrame">The last text edit frame</param>
		/// <returns>A modified text edit frame when needed, otherwise the new text edit frame should be returned</returns>
		public abstract TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame);

		/// <summary>Optional: Called when a change (by user) has happened in rich text</summary>
		/// <param name="richTextEditFrame">The new rich text edit frame</param>
		/// <param name="lastRichTextEditFrame">The last rich text edit frame</param>
		public virtual void OnRichTextEditUpdate(TextEditFrame richTextEditFrame, TextEditFrame lastRichTextEditFrame)
		{
		}
	}
}
