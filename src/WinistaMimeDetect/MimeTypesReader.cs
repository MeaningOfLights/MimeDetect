// ***************************************************************
//  MimeTypesReader   version:  1.0   date: 12/12/2005
//  -------------------------------------------------------------
//  
//  -------------------------------------------------------------
//  Copyright © 2005 Winista - All Rights Reserved
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
using System.Xml;
using System.IO;
using System.Reflection;

namespace Winista.Mime
{
	/// <summary>
	/// Summary description for MimeTypesReader.
	/// </summary>
	public sealed class MimeTypesReader
	{
		/// <summary>
		/// 
		/// </summary>
		public MimeTypesReader()
		{
		}

		internal MimeType[] Read() //System.String filepath)
		{
			MimeType[] types = null;
			//System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            //document.Load(filepath);

            //JT: Read the MimeType Mapping file from the assembly to protect users hacking the mime-types.xml file
            System.Xml.XmlDocument document = GetEmbeddedXml(GetType(), "mime-types.xml");

			types = Visit(document);
			return types;
			
		}


        public static XmlDocument GetEmbeddedXml(Type type, string fileName)
        {
            Stream str = GetEmbeddedFile(type, fileName);
            XmlTextReader tr = new XmlTextReader(str);
            XmlDocument xml = new XmlDocument();
            xml.Load(tr);
            return xml;
        }

        public static Stream GetEmbeddedFile(Assembly assembly, string fileName)
        {
            string assemblyName = assembly.GetName().Name;
            return GetEmbeddedFile(assemblyName, fileName);
        }

        public static Stream GetEmbeddedFile(Type type, string fileName)
        {
            string assemblyName = type.Assembly.GetName().Name;
            return GetEmbeddedFile(assemblyName, fileName);
        }

        /// <summary>
        /// Extracts an embedded file out of a given assembly.
        /// </summary>
        /// <param name="assemblyName">The namespace of you assembly.</param>
        /// <param name="fileName">The name of the file to extract.</param>
        /// <returns>A stream containing the file data.</returns>
        public static Stream GetEmbeddedFile(string assemblyName, string fileName)
        {
            try
            {
                System.Reflection.Assembly a = System.Reflection.Assembly.Load(assemblyName);
                Stream str = a.GetManifestResourceStream(assemblyName + "." + fileName);

                if (str == null)
                    throw new Exception("Could not locate embedded resource '" + fileName + "' in assembly '" + assemblyName + "'");
                return str;
            }
            catch (Exception e)
            {
                throw new Exception(assemblyName + ": " + e.Message);
            }
        }

		/// <summary>Scan through the document. </summary>
		private MimeType[] Visit(System.Xml.XmlDocument document)
		{
			MimeType[] types = null;
			System.Xml.XmlElement element = (System.Xml.XmlElement) document.DocumentElement;
			if ((element != null) && element.Name.Equals("mime-types"))
			{
				types = ReadMimeTypes(element);
			}
			return (types == null) ? (new MimeType[0]) : types;
		}

		private MimeType[] ReadMimeTypes(System.Xml.XmlElement element)
		{
			System.Collections.ArrayList types = new System.Collections.ArrayList();
			System.Xml.XmlNodeList nodes = element.ChildNodes;
			for (int i = 0; i < nodes.Count; i++)
			{
				System.Xml.XmlNode node = nodes.Item(i);
				if (System.Convert.ToInt16(node.NodeType) == (short) System.Xml.XmlNodeType.Element)
				{
					System.Xml.XmlElement nodeElement = (System.Xml.XmlElement) node;
					if (nodeElement.Name.Equals("mime-type"))
					{
						MimeType type = ReadMimeType(nodeElement);
						if (type != null)
						{
							types.Add(type);
						}
					}
				}
			}
            return (MimeType[])SupportUtil.ToArray(types, new MimeType[types.Count]);
		}

		/// <summary>Read Element named mime-type. </summary>
		private MimeType ReadMimeType(System.Xml.XmlElement element)
		{
			System.String name = null;
			System.String description = null;
			MimeType type = null;
			System.Xml.XmlNamedNodeMap attrs = (System.Xml.XmlAttributeCollection) element.Attributes;
			for (int i = 0; i < attrs.Count; i++)
			{
				System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) attrs.Item(i);
				if (attr.Name.Equals("name"))
				{
					name = attr.Value;
				}
				else if (attr.Name.Equals("description"))
				{
					description = attr.Value;
				}
			}
			if ((name == null) || (name.Trim().Equals("")))
			{
				return null;
			}
			
			try
			{
                //System.Diagnostics.Trace.WriteLine("Mime type:" + name);
				type = new MimeType(name);
			}
			catch (MimeTypeException mte)
			{
				// Mime Type not valid... just ignore it
				System.Diagnostics.Trace.WriteLine(mte.ToString() + " ... Ignoring!");
				return null;
			}

			type.Description = description;
			
			System.Xml.XmlNodeList nodes = element.ChildNodes;
			for (int i = 0; i < nodes.Count; i++)
			{
				System.Xml.XmlNode node = nodes.Item(i);
				if (System.Convert.ToInt16(node.NodeType) == (short) System.Xml.XmlNodeType.Element)
				{
					System.Xml.XmlElement nodeElement = (System.Xml.XmlElement) node;
					if (nodeElement.Name.Equals("ext"))
					{
						ReadExt(nodeElement, type);
					}
					else if (nodeElement.Name.Equals("magic"))
					{
						ReadMagic(nodeElement, type);
					}
				}
			}
			return type;
		}

		/// <summary>Read Element named ext. </summary>
		private void  ReadExt(System.Xml.XmlElement element, MimeType type)
		{
			System.Xml.XmlNodeList nodes = element.ChildNodes;
			for (int i = 0; i < nodes.Count; i++)
			{
				System.Xml.XmlNode node = nodes.Item(i);
				if (System.Convert.ToInt16(node.NodeType) == (short) System.Xml.XmlNodeType.Text)
				{
					type.AddExtension(((System.Xml.XmlText) node).Data);
				}
			}
		}

		/// <summary>Read Element named magic. </summary>
		private void  ReadMagic(System.Xml.XmlElement element, MimeType mimeType)
		{
			// element.getValue();
			System.String offset = null;
			System.String content = null;
			System.String type = null;
			System.Xml.XmlNamedNodeMap attrs = (System.Xml.XmlAttributeCollection) element.Attributes;
			for (int i = 0; i < attrs.Count; i++)
			{
				System.Xml.XmlAttribute attr = (System.Xml.XmlAttribute) attrs.Item(i);
				if (attr.Name.Equals("offset"))
				{
					offset = attr.Value;
				}
				else if (attr.Name.Equals("type"))
				{
					type = attr.Value;
                    if (String.Compare(type, "byte", true) == 0)
                    {
                        type = "System.Byte";
                    }
				}
				else if (attr.Name.Equals("value"))
				{
					content = attr.Value;
				}
			}
			if ((offset != null) && (content != null))
			{
				mimeType.AddMagic(System.Int32.Parse(offset), type, content);
			}
		}
	}
}
