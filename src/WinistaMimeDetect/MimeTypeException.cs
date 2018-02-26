// ***************************************************************
//  MimeTypeException   version:  1.0   Date: 12/12/2005
//  -------------------------------------------------------------
//  
//  -------------------------------------------------------------
//  Copyright © 2005 - Winista, All Rights Reserved
// ***************************************************************
// 
// ***************************************************************

/// <summary>
/// Created By: Jeremy Thompson
/// Created Date: 15/03/2010
/// Description: Detect file format - I combined Winista's MimeDetect with URLMon
/// </summary>
/// <remarks></remarks>
using System;

namespace Winista.Mime
{
	/// <summary>
	/// Summary description for MimeTypeException.
	/// </summary>
	public class MimeTypeException : System.ApplicationException
	{
		#region Class Constructor
		/// <summary>
		/// 
		/// </summary>
		public MimeTypeException()
			:base("Mime detection exception")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strMsg"></param>
		public MimeTypeException(String strMsg)
			:base(strMsg)
		{
		}
		#endregion
	}
}
