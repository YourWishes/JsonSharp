using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Copyright (C) Dominic Masters - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 * 
 * Written by Dominic Masters <dominic@domsplace.com>, November 2016
*/
namespace JsonSharp {
    public interface ISerializable {
        Dictionary<String, Object> serialize();
        void deserialize(Dictionary<String, Object> values);
    }
}
