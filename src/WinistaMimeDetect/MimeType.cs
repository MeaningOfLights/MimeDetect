// ***************************************************************
//  MimeType   version:  1.0   Date: 12/12/2005
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
using System.Collections;

namespace Winista.Mime
{
	/// <summary>
    /// Defines a Mime Content Type
	/// </summary>
	public sealed class MimeType
	{
		#region Private Members
		/// <summary>The primary and sub types separator </summary>
		private const System.String SEPARATOR = "/";
		
		/// <summary>The parameters separator </summary>
		private const System.String PARAMS_SEP = ";";
		
		/// <summary>Special characters not allowed in content types. </summary>
		private const System.String SPECIALS = "()<>@,;:\\\"/[]?=";
				
		/// <summary>The Mime-Type full name </summary>
		private System.String m_strName = null;
		
		/// <summary>The Mime-Type primary type </summary>
		private System.String m_strPrimary = null;
		
		/// <summary>The Mime-Type sub type </summary>
		private System.String m_strSub = null;
		
		/// <summary>The Mime-Type description </summary>
		private System.String m_strDescription = null;
		
		/// <summary>The Mime-Type associated extensions </summary>
		private System.Collections.ArrayList m_collExtensions = null;
		
		/// <summary>The magic bytes associated to this Mime-Type </summary>
		private System.Collections.ArrayList m_collMagics = null;
		
		/// <summary>The minimum length of data to provides for magic analyzis </summary>
		private Int32 m_iMinLength = 0;

		#endregion

		#region Class Constructor
		/// <summary> Creates a MimeType from a String.</summary>
		/// <param name="name">the MIME content type String.
		/// </param>
		public MimeType(System.String name)
		{
			if (name == null || name.Length <= 0)
			{
				throw new MimeTypeException("The type can not be null or empty");
			}
			
			// Split the two parts of the Mime Content Type
			System.String[] parts = name.Split(new char[]{SEPARATOR[0]}, 2);
			
			// Checks validity of the parts
			if (parts.Length != 2)
			{
				throw new MimeTypeException("Invalid Content Type " + name);
			}

			Init(parts[0], parts[1]);
		}

		/// <summary> Creates a MimeType with the given primary type and sub type.</summary>
		/// <param name="primary">the content type primary type.
		/// </param>
		/// <param name="sub">the content type sub type.
		/// </param>
		public MimeType(System.String primary, System.String sub)
		{
			Init(primary, sub);
		}
		#endregion

		#region Public Properties
		/// <summary> Return the name of this mime-type.</summary>
		/// <returns> the name of this mime-type.
		/// </returns>
		public System.String Name
		{
			get
			{
				return m_strName;
			}
			
		}
		/// <summary> Return the primary type of this mime-type.</summary>
		/// <returns> the primary type of this mime-type.
		/// </returns>
		public System.String PrimaryType
		{
			get
			{
				return m_strPrimary;
			}
			
		}
		/// <summary> Return the sub type of this mime-type.</summary>
		/// <returns> the sub type of this mime-type.
		/// </returns>
		public System.String SubType
		{
			get
			{
				return m_strSub;
			}
			
		}

		/// <summary> Return the description of this mime-type.</summary>
		/// <returns> the description of this mime-type.
		/// </returns>
		/// <summary> Set the description of this mime-type.</summary>
		/// <param name="description">the description of this mime-type.
		/// </param>
        public System.String Description
		{
			get
			{
				return m_strDescription;
			}
			
			set
			{
				this.m_strDescription = value;
			}
			
		}
		/// <summary> Return the extensions of this mime-type</summary>
		/// <returns> the extensions associated to this mime-type.
		/// </returns>
		public System.String[] Extensions
		{
			get
			{
				return (System.String[]) SupportUtil.ToArray(m_collExtensions, new System.String[m_collExtensions.Count]);
			}
			
		}
		internal int MinLength
		{
			get
			{
				return m_iMinLength;
			}
			
		}
		#endregion

		#region Public Methods
		/// <summary> Cleans a content-type.
		/// This method cleans a content-type by removing its optional parameters
		/// and returning only its <code>primary-type/sub-type</code>.
		/// </summary>
		/// <param name="type">is the content-type to clean.
		/// </param>
		/// <returns> the cleaned version of the specified content-type.
		/// </returns>
		/// <throws>  MimeTypeException if something wrong occurs during the </throws>
		/// <summary>         parsing/cleaning of the specified type.
		/// </summary>
		public static System.String Clean(System.String type)
		{
			return (new MimeType(type)).Name;
		}

		public override System.String ToString()
		{
			return Name;
		}

		/// <summary> Indicates if an object is equal to this mime-type.
		/// The specified object is equal to this mime-type if it is not null, and
		/// it is an instance of MimeType and its name is equals to this mime-type.
		/// 
		/// </summary>
		/// <param name="obj">the reference object with which to compare.
		/// </param>
		/// <returns> <code>true</code> if this mime-type is equal to the object
		/// argument; <code>false</code> otherwise.
		/// </returns>
		public  override bool Equals(System.Object obj)
		{
			try
			{
				return ((MimeType) obj).Name.Equals(this.Name);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		#endregion

		#region Internal Methods
		/// <summary> Add a supported extension.</summary>
		/// <param name="the">extension to add to the list of extensions associated
		/// to this mime-type.
		/// </param>
		internal void AddExtension(System.String ext)
		{
			m_collExtensions.Add(ext);
		}
		
		internal void AddMagic(int offset, System.String type, System.String magic)
		{
			// Some preliminary checks...
			if ((magic == null) || (magic.Length < 1))
			{
				return ;
			}
			Magic m = new Magic(this, offset, type, magic);
			if (m != null)
			{
				m_collMagics.Add(m);
				m_iMinLength = System.Math.Max(m_iMinLength, m.Size());
			}
		}
		
		internal bool HasMagic()
		{
			return (m_collMagics.Count > 0);
		}
		
		public bool Matches(System.String url)
		{
			bool match = false;
			int index = url.LastIndexOf((System.Char) '.');
			if ((index != - 1) && (index < url.Length - 1))
			{
				// There's an extension, so try to find if it matches mines
				match = m_collExtensions.Contains(url.Substring(index + 1));
			}
			return match;
		}

		internal bool Matches(sbyte[] data)
		{
			if (!HasMagic())
			{
				return false;
			}
			
			Magic tested = null;
			for (int i = 0; i < m_collMagics.Count; i++)
			{
				tested = (Magic) m_collMagics[i];
				if (tested.Matches(data))
				{
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Helper Methods
		/// <summary>Init method used by constructors. </summary>
		private void  Init(System.String primary, System.String sub)
		{
			// Preliminary checks...
			if ((primary == null) || (primary.Length <= 0) || (!IsValid(primary)))
			{
				throw new MimeTypeException("Invalid Primary Type " + primary);
			}
			// Remove optional parameters from the sub type
			System.String clearedSub = null;
			if (sub != null)
			{
				clearedSub = sub.Split(PARAMS_SEP[0])[0];
			}
			if ((clearedSub == null) || (clearedSub.Length <= 0) || (!IsValid(clearedSub)))
			{
				throw new MimeTypeException("Invalid Sub Type " + clearedSub);
			}
			
			// All is ok, assign values
			this.m_strName = primary + SEPARATOR + clearedSub;
			this.m_strPrimary = primary;
			this.m_strSub = clearedSub;
			this.m_collExtensions = new System.Collections.ArrayList();
			this.m_collMagics = new System.Collections.ArrayList();
		}

		/// <summary>Checks if the specified primary or sub type is valid. </summary>
		private bool IsValid(System.String type)
		{
			return (type != null) && (type.Trim().Length > 0) && !HasCtrlOrSpecials(type);
		}
		
		/// <summary>Checks if the specified string contains some special characters. </summary>
		private bool HasCtrlOrSpecials(System.String type)
		{
			int len = type.Length;
			int i = 0;
			while (i < len)
			{
				char c = type[i];
				if (c <= '\x001A' || SPECIALS.IndexOf((System.Char) c) > 0)
				{
					return true;
				}
				i++;
			}
			return false;
		}
		#endregion

		#region Magic Class
 		private class Magic
		{
			private int m_iOffset;
			private sbyte[] m_collMagic = null;
            private MimeType m_obMimeType;
			
			internal Magic(MimeType obMimeType, int offset, System.String type, System.String magic)
			{
                if (null == obMimeType)
                {
                    throw new ArgumentNullException("Null MimeType object");
                }
                m_obMimeType = obMimeType;
				this.m_iOffset = offset;
				
				if ((type != null) && (type.Equals("System.Byte")))
				{
					this.m_collMagic = ReadBytes(magic);
				}
				else
				{
                    this.m_collMagic = SupportUtil.ToSByteArray(SupportUtil.ToByteArray(magic));
				}
			}
			
			internal int Size()
			{
				return (m_iOffset + m_collMagic.Length);
			}
			
			internal bool Matches(sbyte[] data)
			{
				if (data == null)
				{
					return false;
				}
				
				int idx = m_iOffset;
				if ((idx + m_collMagic.Length) > data.Length)
				{
					return false;
				}
				
				for (int i = 0; i < m_collMagic.Length; i++)
				{
					if (m_collMagic[i] != data[idx++])
					{
						return false;
					}
				}
				return true;
			}
			
			private sbyte[] ReadBytes(System.String magic)
			{
				sbyte[] data = null;
				
				if ((magic.Length % 2) == 0)
				{
					System.String tmp = magic.ToLower();
					data = new sbyte[tmp.Length / 2];
					int byteValue = 0;
					for (int i = 0; i < tmp.Length; i++)
					{
						char c = tmp[i];
						int number;
						if (c >= '0' && c <= '9')
						{
							number = c - '0';
						}
						else if (c >= 'a' && c <= 'f')
						{
							number = 10 + c - 'a';
						}
						else
						{
							throw new System.ArgumentException();
						}
						if ((i % 2) == 0)
						{
							byteValue = number * 16;
						}
						else
						{
							byteValue += number;
							data[i / 2] = (sbyte) byteValue;
						}
					}
				}
				return data;
			}
			
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
			public override System.String ToString()
			{
				System.Text.StringBuilder buf = new System.Text.StringBuilder();
				buf.Append("[").Append(m_iOffset).Append("/").Append(SupportUtil.ToByteArray(m_collMagic)).Append("]");
				return buf.ToString();
			}
		}
		#endregion
	}
}
