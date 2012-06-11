using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoarIMPRINTPlugin
{
	// a class for convenience methods on SML classes
	public static class SMLExtension
	{
		// get a command link by name
		public static sml.Identifier GetCommand(this sml.Agent agent, string command_name)
		{
			// examine each command
			for (int i = 0; i < agent.GetNumberCommands(); i++)
			{
				sml.Identifier id = agent.GetCommand(i);
				// if command name matches the one we're looking fore, return the Identifier
				if (id.GetCommandName() == command_name)
				{
					return id;
				}
			}
			return null;
		}

		// get typed children of links
		public static string FindStringByAttribute(this sml.Identifier element, string attribute, int index = 0)
		{
			// find the generic element by name
			sml.WMElement child = element.FindByAttribute(attribute, index);
			// return null if it doesn't exist
			if (child == null) return null;
			// cast the element to a string element
			sml.StringElement stringChild = child.ConvertToStringElement();
			// return null if cast fails
			if (stringChild == null) return null;
			// return the string value of the element
			return stringChild.GetValue();
		}
		// exceptions to throw in failure cases of getting natively typed children
		public class AttributeNotFoundException : SystemException
		{
			public string attribute;
			public sml.Identifier element;
			public AttributeNotFoundException(string attr, sml.Identifier el)
			{
				attribute = attr;
				element = el;
			}
		}
		public class InvalidElementTypeException : SystemException
		{
			public string type;
			public sml.WMElement element;
			public InvalidElementTypeException(string t, sml.WMElement el)
			{
				type = t;
				element = el;
			}
		}
		// get an int typed child
		public static long FindIntByAttribute(this sml.Identifier element, string attribute, int index = 0)
		{
			// find the generic element by name
			sml.WMElement child = element.FindByAttribute(attribute, index);
			// throw exception if it doesn't exist
			if (child == null) throw new AttributeNotFoundException(attribute, element);
			// cast the element to an int element
			sml.IntElement intChild = child.ConvertToIntElement();
			// throw exception if cast fails
			if (intChild == null) throw new InvalidElementTypeException("int", child);
			// return the int value of the element
			return intChild.GetValue();
		}
		// get a float typed child
		public static double FindFloatByAttribute(this sml.Identifier element, string attribute, int index = 0)
		{
			// find the generic element by name
			sml.WMElement child = element.FindByAttribute(attribute, index);
			// throw exception if it doesn't exist
			if (child == null) throw new AttributeNotFoundException(attribute, element);
			// cast the element to a fload element
			sml.FloatElement floatChild = child.ConvertToFloatElement();
			// throw exception if cast fails
			if (floatChild == null) throw new InvalidElementTypeException("float", child);
			// return the float value of the element
			return floatChild.GetValue();
		}
		// get an identifier typed childe
		public static sml.Identifier FindIDByAttribute(this sml.Identifier element, string attribute, int index = 0)
		{
			// find the generic element by name
			sml.WMElement child = element.FindByAttribute(attribute, index);
			// return null if it doesn't exist
			if (child == null) return null;
			// cast the element to an Identifier
			sml.Identifier idChild = child.ConvertToIdentifier();
			// return the casted ID. If cast fails, this will be nulls
			return idChild;
		}
		// get iterable children. optionally by attribute
		public static IEnumerable<sml.WMElement> GetChildren(this sml.Identifier element, string attribute = null)
		{
			// list to hold children
			List<sml.WMElement> children = new List<sml.WMElement>();

			if (attribute == null)
			{
				int numChildren = element.GetNumberChildren();
				// add all children
				for (int i = 0; i < numChildren; i++)
				{
					children.Add(element.GetChild(i));
				}
			}
			else
			{
				// add children at attribute
				sml.WMElement child;
				int i = 0;
				// wow this is horrifying. TODO CHECK THIS
				while ((child = element.FindByAttribute(attribute, i++)) != null) children.Add(child);
			}

			return children;
		}
		// get typed iterable children. optionally by attribute
		// these all use LINQ queries to retrieve correct children
		public static IEnumerable<sml.Identifier> GetIDChildren(this sml.Identifier element, string attribute = null)
		{
			return
				from child in element.GetChildren(attribute)
				where child.IsIdentifier()
				select child.ConvertToIdentifier();
		}
		public static IEnumerable<double> GetFloatChildren(this sml.Identifier element, string attribute = null)
		{
			return
				from child in element.GetChildren(attribute)
				where child.GetValueType() == "float"
				select child.ConvertToFloatElement().GetValue();
		}
		public static IEnumerable<long> GetIntChildren(this sml.Identifier element, string attribute = null)
		{
			return
				from child in element.GetChildren(attribute)
				where child.GetValueType() == "int"
				select child.ConvertToIntElement().GetValue();
		}
		public static IEnumerable<string> GetStringChildren(this sml.Identifier element, string attribute = null)
		{
			return
				from child in element.GetChildren(attribute)
				where child.GetValueType() == "string"
				select child.ConvertToStringElement().GetValue();
		}

		// return a WMElement at an attribute path in soar syntax
		// at each entry in the path, the first child at the attribute is used
		public static sml.WMElement GetChildAtAttributePath(this sml.Identifier element, string attributePath)
		{
			string[] members = attributePath.Split(new[] { '.', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
			return GetChildAtAttributePathComponents(element, members);
		}
		// return a value at a path of member names held in the members array
		private static sml.WMElement GetChildAtAttributePathComponents(sml.Identifier element, string[] attributes)
		{
			// go through path and get children
			sml.Identifier current = element;
			foreach (string attribute in attributes.Take(attributes.Length - 1))
			{
				current = current.FindIDByAttribute(attribute);
				// if there was no Identifier found there, return null
				if (current == null)
				{
					return null;
				}
			}
			// get the last element
			return current.FindByAttribute(attributes.Last(),0);
		}
		// return typed values at attribute paths
		public static long GetIntAtAttributePath(this sml.Identifier element, string attributePath)
		{
			sml.IntElement intElement = GetChildAtAttributePath(element, attributePath).ConvertToIntElement();
			if (intElement == null)
			{
				throw new InvalidElementTypeException("int", GetChildAtAttributePath(element, attributePath));
			}
			return intElement.GetValue();
		}
		public static double GetFloatAtAttributePath(this sml.Identifier element, string attributePath)
		{
			sml.FloatElement floatElement = GetChildAtAttributePath(element, attributePath).ConvertToFloatElement();
			if (floatElement == null)
			{
				throw new InvalidElementTypeException("float", GetChildAtAttributePath(element, attributePath));
			}
			return floatElement.GetValue();
		}
		public static string GetStringAtAttributePath(this sml.Identifier element, string attributePath)
		{
			sml.StringElement stringElement = GetChildAtAttributePath(element, attributePath).ConvertToStringElement();
			if (stringElement == null)
			{
				return null;
			}
			return stringElement.GetValue();
		}
		public static sml.Identifier GetIDAtAttributePath(this sml.Identifier element, string attributePath)
		{
			return GetChildAtAttributePath(element, attributePath).ConvertToIdentifier();
		}
	}
}
