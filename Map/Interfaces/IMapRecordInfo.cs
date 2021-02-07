using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSS.Mapping
{
    /// <summary>
    /// Defines an object that has context information about an <see cref="IMapData"/> such as SateID, CountyID, Name etc..
    /// </summary>
    public interface IMapRecordInfo
    {
        /// <summary>
        /// The record number of the record, normally the lineNumber from the converted dbf file
        /// </summary>
        int RecordNumber { get; }

        /// <summary>
        /// The ID Value of the Record, usually the state ID, County ID etc...
        /// <para>
        /// <see cref="PrimaryID"/> may be a "County ID" and <see cref="SecondaryID"/> may refer to "State ID", this would provide contextual information that the example object was a County within a State, and further provide information as to WHAT county in WHICH state.
        /// </para>
        /// </summary>
        int PrimaryID { get; }

        /// <summary>
        /// The ID Value of the record that normally contains contextual information about the ID
        /// <para>
        /// <see cref="PrimaryID"/> may be a "County ID" and <see cref="SecondaryID"/> may refer to "State ID", this would provide contextual information that the example object was a County within a State, and further provide information as to WHAT county in WHICH state.
        /// </para>
        /// </summary>
        int SecondaryID { get; }

        /// <summary>
        /// The Name of the record, usually the name of whatever area the record is supposed to represent such as "Florida" or "New York" may be a number such as Block "12547".
        /// </summary>
        string Name { get;}
    }
}
