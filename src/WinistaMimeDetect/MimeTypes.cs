// ***************************************************************
//  MimeTypes   version:  1.0   Date: 12/12/2005
//  -------------------------------------------------------------
//  
//  -------------------------------------------------------------
//  Copyright © 2005 - Winista All Rights Reserved
// ***************************************************************
// 
// ***************************************************************

/// <summary>
/// Created By: Jeremy Thompson
/// Created Date: 15/03/2010
/// Description: Detect file format - I combined Winista's MimeDetect with URLMon
/// </summary>
/// <remarks></remarks>
/// 
using System;
using System.IO;

namespace Winista.Mime
{
    /// <summary>
    /// Summary description for MimeTypes.
    /// </summary>
    public sealed class MimeTypes
    {
        #region Class Members
        /// <summary>The default <code>application/octet-stream</code> MimeType </summary>
        public const System.String DEFAULT = "application/octet-stream";

        /// <summary>All the registered MimeTypes </summary>
        private System.Collections.ArrayList types = new System.Collections.ArrayList();

        /// <summary>All the registered MimeType indexed by name </summary>
        private System.Collections.Hashtable typesIdx = new System.Collections.Hashtable();

        /// <summary>MimeTypes indexed on the file extension </summary>
        private System.Collections.IDictionary extIdx = new System.Collections.Hashtable();

        /// <summary>List of MimeTypes containing a magic char sequence </summary>
        private System.Collections.IList magicsIdx = new System.Collections.ArrayList();

        /// <summary>The minimum length of data to provide to check all MimeTypes </summary>
        private int m_iMinLength = 0;


        /// <summary> My registered instances
        /// There is one instance associated for each specified file while
        /// calling the {@link #get(String)} method.
        /// Key is the specified file path in the {@link #get(String)} method.
        /// Value is the associated MimeType instance.
        /// </summary>
        private static System.Collections.IDictionary instances = new System.Collections.Hashtable();
        #endregion

        /// <summary>Should never be instanciated from outside </summary>
        public MimeTypes()
        {
            MimeTypesReader reader = new MimeTypesReader();
            Add(reader.Read());
        }

        /// <summary> Return the minimum length of data to provide to analyzing methods
        /// based on the document's content in order to check all the known
        /// MimeTypes.
        /// </summary>
        /// <returns> the minimum length of data to provide.
        /// </returns>
        public int MinLength
        {
            get
            {
                return m_iMinLength;
            }

        }

        /// <summary> Return a MimeTypes instance.</summary>
        /// <param name="filepath">is the mime-types definitions xml file.
        /// </param>
        /// <returns> A MimeTypes instance for the specified filepath xml file.
        /// </returns>
        public static MimeTypes Get(System.String filepath)
        {
            MimeTypes instance = null;
            lock (instances.SyncRoot)
            {
                instance = (MimeTypes)instances[filepath];
                if (instance == null)
                {
                    //instance = new MimeTypes(filepath, null);
                    instance = new MimeTypes();
                    instances[filepath] = instance;
                }
            }
            return instance;
        }

        /// <summary> Find the Mime Content Type of a document from its URL.</summary>
        /// <param name="url">of the document to analyze.
        /// </param>
        /// <returns> the Mime Content Type of the specified document URL, or
        /// <code>null</code> if none is found.
        /// </returns>
        public MimeType GetMimeType(System.Uri url)
        {
            return GetMimeType(url.AbsolutePath);
        }

        /// <summary> Find the Mime Content Type of a document from its name.</summary>
        /// <param name="name">of the document to analyze.
        /// </param>
        /// <returns> the Mime Content Type of the specified document name, or
        /// <code>null</code> if none is found.
        /// </returns>
        public MimeType GetMimeType(System.String name)
        {
            MimeType[] founds = GetMimeTypes(name);
            if ((founds == null) || (founds.Length < 1))
            {
                // No mapping found, just return null
                return null;
            }
            else
            {
                // Arbitraly returns the first mapping
                return founds[0];
            }
        }

        public MimeType GetMimeTypeFromFile(string filePath)
        {
            sbyte[] fileData = null;
            using (FileStream srcFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[srcFile.Length];
                srcFile.Read(data, 0, (Int32)srcFile.Length);
                fileData = Winista.Mime.SupportUtil.ToSByteArray(data);
            }

            MimeType oMimeType = GetMimeType(fileData);
            if (oMimeType != null) return oMimeType;

            //We haven't found the file using Magic (eg a text/plain file)
            //so instead use URLMon to try and get the files format
            Winista.MimeDetect.URLMONMimeDetect.urlmonMimeDetect urlmonMimeDetect = new Winista.MimeDetect.URLMONMimeDetect.urlmonMimeDetect();
            string urlmonMimeType = urlmonMimeDetect.GetMimeFromFile(filePath);
            if (!string.IsNullOrEmpty(urlmonMimeType))
            {
                foreach (MimeType mimeType in types)
                {
                    if (mimeType.Name == urlmonMimeType)
                    {
                        return mimeType;
                    }
                }
            }

            return oMimeType;
        }
        /// <summary> Find the Mime Content Type of a stream from its content.
        /// 
        /// </summary>
        /// <param name="data">are the first bytes of data of the content to analyze.
        /// Depending on the length of provided data, all known MimeTypes are
        /// checked. If the length of provided data is greater or egals to
        /// the value returned by {@link #getMinLength()}, then all known
        /// MimeTypes are checked, otherwise only the MimeTypes that could be
        /// analyzed with the length of provided data are analyzed.
        /// 
        /// </param>
        /// <returns> The Mime Content Type found for the specified data, or
        /// <code>null</code> if none is found.
        /// </returns>
        /// <seealso cref="#getMinLength()">
        /// </seealso>
        /// 
        public MimeType GetMimeType(sbyte[] data)
        {
            // Preliminary checks
            if ((data == null) || (data.Length < 1))
            {
                return null;
            }
            System.Collections.IEnumerator iter = magicsIdx.GetEnumerator();
            MimeType type = null;
            // TODO: This is a very naive first approach (scanning all the magic
            //       bytes since one is matching.
            //       A first improvement could be to use a search path on the magic
            //       bytes.
            // TODO: A second improvement could be to search for the most qualified
            //       (the longuest) magic sequence (not the first that is matching).
            while (iter.MoveNext())
            {
                type = (MimeType)iter.Current;
                if (type.Matches(data))
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary> Find the Mime Content Type of a document from its name and its content.
        /// 
        /// </summary>
        /// <param name="name">of the document to analyze.
        /// </param>
        /// <param name="data">are the first bytes of the document's content.
        /// </param>
        /// <returns> the Mime Content Type of the specified document, or
        /// <code>null</code> if none is found.
        /// </returns>
        /// <seealso cref="#getMinLength()">
        /// </seealso>
        public MimeType GetMimeType(System.String name, sbyte[] data)
        {

            // First, try to get the mime-type from the name
            MimeType mimeType = null;
            MimeType[] mimeTypes = GetMimeTypes(name);
            if (mimeTypes == null)
            {
                // No mime-type found, so trying to analyse the content
                mimeType = GetMimeType(data);
            }
            else if (mimeTypes.Length > 1)
            {
                // TODO: More than one mime-type found, so trying magic resolution
                // on these mime types
                //mimeType = getMimeType(data, mimeTypes);
                // For now, just get the first one
                mimeType = mimeTypes[0];
            }
            else
            {
                mimeType = mimeTypes[0];
            }
            return mimeType;
        }

        /// <summary> Return a MimeType from its name.</summary>
        public MimeType ForName(System.String name)
        {
            return (MimeType)typesIdx[name];
        }

        /// <summary> Add the specified mime-types in the repository.</summary>
        /// <param name="types">are the mime-types to add.
        /// </param>
        internal void Add(MimeType[] types)
        {
            if (types == null)
            {
                return;
            }
            for (int i = 0; i < types.Length; i++)
            {
                Add(types[i]);
            }
        }

        /// <summary> Add the specified mime-type in the repository.</summary>
        /// <param name="type">is the mime-type to add.
        /// </param>
        internal void Add(MimeType type)
        {
            typesIdx[type.Name] = type;
            types.Add(type);
            // Update minLentgth
            m_iMinLength = System.Math.Max(m_iMinLength, type.MinLength);
            // Update the extensions index...
            System.String[] exts = type.Extensions;
            if (exts != null)
            {
                for (int i = 0; i < exts.Length; i++)
                {
                    System.Collections.IList list = (System.Collections.IList)extIdx[exts[i]];
                    if (list == null)
                    {
                        // No type already registered for this extension...
                        // So, create a list of types
                        list = new System.Collections.ArrayList();
                        extIdx[exts[i]] = list;
                    }
                    list.Add(type);
                }
            }
            // Update the magics index...
            if (type.HasMagic())
            {
                magicsIdx.Add(type);
            }
        }

        /// <summary> Returns an array of matching MimeTypes from the specified name
        /// (many MimeTypes can have the same registered extensions).
        /// </summary>
        private MimeType[] GetMimeTypes(System.String name)
        {
            System.Collections.IList mimeTypes = null;
            int index = name.LastIndexOf((System.Char)'.');
            if ((index != -1) && (index != name.Length - 1))
            {
                // There's an extension, so try to find
                // the corresponding mime-types
                System.String ext = name.Substring(index + 1);
                mimeTypes = (System.Collections.IList)extIdx[ext];
            }

            return (mimeTypes != null) ? (MimeType[])SupportUtil.ToArray(mimeTypes, new MimeType[mimeTypes.Count]) : null;
        }
    }
}
