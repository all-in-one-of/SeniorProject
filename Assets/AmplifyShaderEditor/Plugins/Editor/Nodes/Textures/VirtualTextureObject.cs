// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	public enum VirtualPreset
	{
		Unity_Legacy,
		Unity5,
		Alloy,
		UBER,
		Skyshop,
		Lux
	}
	
	public enum VirtualChannel
	{
		Albedo = 0,
		Base,
		Normal,
		Height,
		Occlusion,
		Displacement,
		Specular,
		SpecMet,
		Material,
	}

	[Serializable]
	[NodeAttributes( "Virtual Texture Object", "Textures", "Represents a Virtual Texture Asset. Can only be used alongside Texture Sample node by connecting to its Tex Input Port" , null, KeyCode.None, true, false, null, null, false, null, 1 )]
	public class VirtualTexturePropertyNode : TexturePropertyNode
	{
		protected const string VirtualPresetStr = "Layout Preset";
		protected const string VirtualChannelStr = "Virtual Channel";

		private readonly string[] ChannelTypeStr = {
			"Albedo		- Channel 0 - RGBA",
			"Base	- Channel 0 - RGBA",
			"Normal		- Channel 1 - GA     ",
			"Height		- Channel 1 - B        ",
			"Occlusion	- Channel 1 - R        ",
			"Displacement	- Channel 1 - B        ",
			"Specular	- Channel 2 - RGBA",
			"Specular|Metallic	- Channel 2 - RGBA",
			"Material	- Channel 2 - RGBA",};

		private readonly string[] Dummy = { string.Empty };
		private string[] m_channelTypeStr;

		[SerializeField]
		protected VirtualPreset m_virtualPreset = VirtualPreset.Unity5;

		[SerializeField]
		protected VirtualChannel m_virtualChannel = VirtualChannel.Albedo;

		[SerializeField]
		private int m_selectedChannelInt = 0;

		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			m_channelTypeStr = Dummy;
			m_virtualChannel = GetChannel( 0 );
			ChangeChannels();
			UIUtils.AddVirtualTextureCount();
			m_channelTypeStr = Dummy;
		}

		public override void DrawSubProperties()
		{
			ShowDefaults();

			base.DrawSubProperties();
		}

		public override void DrawMaterialProperties()
		{
			ShowDefaults();

			base.DrawMaterialProperties();
		}

		new void ShowDefaults()
		{
			EditorGUI.BeginChangeCheck();
			m_virtualPreset = ( VirtualPreset )EditorGUILayoutEnumPopup( VirtualPresetStr, m_virtualPreset );
			if ( EditorGUI.EndChangeCheck() )
			{
				ChangeChannels();
			}

			EditorGUI.BeginChangeCheck();
			m_selectedChannelInt = EditorGUILayoutPopup( VirtualChannelStr, m_selectedChannelInt, m_channelTypeStr );
			if ( EditorGUI.EndChangeCheck() )
			{
				m_virtualChannel = GetChannel( m_selectedChannelInt );
			}
		}

		private VirtualChannel GetChannel(int popupInt)
		{
			int remapInt = 0;
			switch ( m_virtualPreset )
			{
				case VirtualPreset.Unity_Legacy:
					remapInt = popupInt == 0 ? 1 : popupInt == 1 ? 2 : popupInt == 2 ? 4 : popupInt == 3 ? 5 : 0;
					break;
				default:
				case VirtualPreset.Unity5:
				case VirtualPreset.UBER:
					remapInt = popupInt == 0 ? 0 : popupInt == 1 ? 2 : popupInt == 2 ? 3 : popupInt == 3 ? 4 : popupInt == 4 ? 7 : 0;
					break;
				case VirtualPreset.Alloy:
					remapInt = popupInt == 0 ? 1 : popupInt == 1 ? 2 : popupInt == 2 ? 3 : popupInt == 3 ? 8 : 0;
					break;
				case VirtualPreset.Skyshop:
				case VirtualPreset.Lux:
					remapInt = popupInt == 0 ? 1 : popupInt == 1 ? 2 : popupInt == 2 ? 6 : 0;
					break;
			}

			return ( VirtualChannel ) remapInt;
		}

		private void ChangeChannels()
		{
			m_channelTypeStr = Dummy;
			switch ( m_virtualPreset )
			{
				case VirtualPreset.Unity_Legacy:
					m_channelTypeStr = new string[] { ChannelTypeStr[ 1 ], ChannelTypeStr[ 2 ], ChannelTypeStr[ 4 ], ChannelTypeStr[ 5 ] };
					break;
				default:
				case VirtualPreset.Unity5:
				case VirtualPreset.UBER:
					m_channelTypeStr = new string[] { ChannelTypeStr[ 0 ], ChannelTypeStr[ 2 ], ChannelTypeStr[ 3 ], ChannelTypeStr[ 4 ], ChannelTypeStr[ 7 ] };
					break;
				case VirtualPreset.Alloy:
					m_channelTypeStr = new string[] { ChannelTypeStr[ 1 ], ChannelTypeStr[ 2 ], ChannelTypeStr[ 3 ], ChannelTypeStr[ 8 ] };
					break;
				case VirtualPreset.Skyshop:
				case VirtualPreset.Lux:
					m_channelTypeStr = new string[] { ChannelTypeStr[ 1 ], ChannelTypeStr[ 2 ], ChannelTypeStr[ 6 ] };
					break;
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalVar );
			dataCollector.AddToProperties( m_uniqueId, "[HideInInspector] _VTInfoBlock( \"VT( auto )\", Vector ) = ( 0, 0, 0, 0 )", -1 );

			return PropertyName;
		}

		public override string GetPropertyValue()
		{
			string propertyValue = string.Empty;
			switch ( m_virtualChannel )
			{
				default:
				case VirtualChannel.Albedo:
				case VirtualChannel.Base:
					propertyValue = PropertyName + "(\"" + m_propertyInspectorName + "\", 2D) = \"" + m_defaultTextureValue + "\" {}";
					break;
				case VirtualChannel.Normal:
					propertyValue = PropertyName + "(\"" + m_propertyInspectorName + "\", 2D) = \"" + m_defaultTextureValue + "\" {}";
					break;
				case VirtualChannel.SpecMet:
					propertyValue = PropertyName + "(\"" + m_propertyInspectorName + "\", 2D) = \"" + m_defaultTextureValue + "\" {}";
					break;
			}
			return PropertyAttributes + propertyValue;
		}

		public override string GetUniformValue()
		{
			return "uniform sampler2D " + PropertyName + ";";
		}

		public override void GetUniformData( out string dataType, out string dataName )
		{
			dataType = "sampler2D";
			dataName = PropertyName;
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			string textureName = GetCurrentParam( ref nodeParams );
			m_defaultValue = AssetDatabase.LoadAssetAtPath<Texture>( textureName );
			m_isNormalMap = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			m_defaultTextureValue = ( TexturePropertyValues )Enum.Parse( typeof( TexturePropertyValues ), GetCurrentParam( ref nodeParams ) );
			m_autocastMode = ( AutoCastType )Enum.Parse( typeof( AutoCastType ), GetCurrentParam( ref nodeParams ) );
			m_virtualPreset = ( VirtualPreset )Enum.Parse( typeof( VirtualPreset ), GetCurrentParam( ref nodeParams ) );
			m_selectedChannelInt = Convert.ToInt32( GetCurrentParam( ref nodeParams ) );
			m_virtualChannel = GetChannel( m_selectedChannelInt );
			ChangeChannels();

			m_forceNodeUpdate = true;

			ConfigFromObject( m_defaultValue );
			ConfigureInputPorts();
			ConfigureOutputPorts();
		}

		public override void ReadAdditionalData( ref string[] nodeParams ) { }

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, ( m_defaultValue != null ) ? AssetDatabase.GetAssetPath( m_defaultValue ) : Constants.NoStringValue );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_isNormalMap.ToString() );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_defaultTextureValue );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_autocastMode );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_virtualPreset );
			IOUtils.AddFieldValueToString( ref nodeInfo, m_selectedChannelInt );
		}

		public override void WriteAdditionalToString( ref string nodeInfo, ref string connectionsInfo ) { }

		public override string PropertyName
		{
			get
			{
				string propertyName = string.Empty;
				switch ( m_virtualChannel )
				{
					default:
					case VirtualChannel.Albedo:
					case VirtualChannel.Base:
						propertyName = "_MainTex";
						break;
					case VirtualChannel.Normal:
						propertyName = "_BumpMap";
						break;
					case VirtualChannel.SpecMet:
						propertyName = "_MetallicGlossMap";
						break;
					case VirtualChannel.Occlusion:
						propertyName = "_OcclusionMap";
						break;
				}
				return propertyName;
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			UIUtils.RemoveVirtualTextureCount();
		}

		public VirtualChannel Channel { get { return m_virtualChannel; } }
	}
}
