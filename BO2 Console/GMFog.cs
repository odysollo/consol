using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO2_Console
{

    /// <summary>
    /// Gmzorz's fog mod implemented for Consol by meth.
    /// Credit www.gmzorz.com.
    /// </summary>
    class GMFog
    {

        public enum FogAddresses
        {
            FogStartDist = 0x036434B8,
            FogFadeDist = 0x036434BC,
            FogHeight = 0x036434C4,
            FogBias = 0x036434C8,
            FogBaseColor = 0x036434D8,
            FogFarColor = 0x036434E8
        }

        private ProcessMemory ProcessMemory;

        private float _fogStartDist;

        public float FogStartDist
        {
            get
            {
                return this._fogStartDist;
            }
            set
            {
                ProcessMemory.WriteFloat((int)FogAddresses.FogStartDist, value);
                _fogStartDist = value;
            }
        }

        private float _fogFadeDist;

        public float FogFadeDist
        {
            get
            {
                return this._fogFadeDist;
            }
            set
            {
                ProcessMemory.WriteFloat((int)FogAddresses.FogFadeDist, value);
                _fogFadeDist = value;
            }
        }

        private float _fogHeightDist;

        public float FogHeightDist
        {
            get
            {
                return this._fogHeightDist;
            }
            set
            {
                ProcessMemory.WriteFloat((int)FogAddresses.FogHeight, value);
                _fogHeightDist = value;
            }
        }

        private float _fogBiasDist;

        public float FogBiasDist
        {
            get
            {
                return this._fogBiasDist;
            }
            set
            {
                ProcessMemory.WriteFloat((int)FogAddresses.FogBias, value);
                _fogBiasDist = value;
            }
        }

        private ProcessMemory.Float4 _fogBaseColor;

        public ProcessMemory.Float4 FogBaseColor
        {
            get
            {
                return this._fogBaseColor;
            }
            set
            {
                ProcessMemory.WriteFloat4((int)FogAddresses.FogBaseColor, value);
                _fogBaseColor = value;
            }
        }

        private ProcessMemory.Float4 _fogFarColor;

        public ProcessMemory.Float4 FogFarColor
        {
            get
            {
                return this._fogFarColor;
            }
            set
            {
                ProcessMemory.WriteFloat4((int)FogAddresses.FogFarColor, value);
                _fogFarColor = value;
            }
        }

        public GMFog(ProcessMemory processMemory)
        {
            ProcessMemory = processMemory;
        }
    }
}