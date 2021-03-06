﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SimpleLoad
{
	public class BigNumber
	{
		private UInt32[] number;
		private int size;
		private int maxDigits;

		public BigNumber(int maxDigits)
		{
			this.maxDigits = maxDigits;
			this.size = (int) Math.Ceiling((float) maxDigits*0.104) + 2;
			number = new UInt32[size];
		}

		public BigNumber(int maxDigits, UInt32 intPart) : this(maxDigits)
		{
			number[0] = intPart;
			for (int i = 1; i < size; i++)
			{
				number[i] = 0;
			}
		}

		public void Add(BigNumber value)
		{
			VerifySameSize(value);

			int index = size - 1;
			while (index >= 0 && value.number[index] == 0)
				index--;

			UInt32 carry = 0;
			while (index >= 0)
			{
				UInt64 result = (UInt64)number[index] +
								value.number[index] + carry;
				number[index] = (UInt32)result;
				if (result >= 0x100000000U)
					carry = 1;
				else
					carry = 0;
				index--;
			}
		}

		public void Subtract(BigNumber value)
		{
			VerifySameSize(value);

			int index = size - 1;
			while (index >= 0 && value.number[index] == 0)
				index--;

			UInt32 borrow = 0;
			while (index >= 0)
			{
				UInt64 result = 0x100000000U + (UInt64)number[index] -
								value.number[index] - borrow;
				number[index] = (UInt32)result;
				if (result >= 0x100000000U)
					borrow = 0;
				else
					borrow = 1;
				index--;
			}
		}

		public void Multiply(UInt32 value)
		{
			int index = size - 1;
			while (index >= 0 && number[index] == 0)
				index--;

			UInt32 carry = 0;
			while (index >= 0)
			{
				UInt64 result = (UInt64)number[index] * value + carry;
				number[index] = (UInt32)result;
				carry = (UInt32)(result >> 32);
				index--;
			}
		}

		public void Divide(UInt32 value)
		{
			int index = 0;
			while (index < size && number[index] == 0)
				index++;

			UInt32 carry = 0;
			while (index < size)
			{
				UInt64 result = number[index] + ((UInt64)carry << 32);
				number[index] = (UInt32)(result / (UInt64)value);
				carry = (UInt32)(result % (UInt64)value);
				index++;
			}
		}

		private void VerifySameSize(BigNumber value)
		{
			if (Object.ReferenceEquals(this, value))
				throw new Exception("BigNumbers cannot operate on themselves");
			if (value.size != this.size)
				throw new Exception("BigNumbers must have the same size");
		}

		public void Assign(BigNumber value)
		{
			VerifySameSize(value);
			for (int i = 0; i < size; i++)
			{
				number[i] = value.number[i];
			}
		}

		public void ArcTan(UInt32 multiplicand, UInt32 reciprocal)
		{
			BigNumber X = new BigNumber(maxDigits, multiplicand);
			X.Divide(reciprocal);
			reciprocal *= reciprocal;

			this.Assign(X);

			BigNumber term = new BigNumber(maxDigits);
			UInt32 divisor = 1;
			bool subtractTerm = true;
			while (true)
			{
				X.Divide(reciprocal);
				term.Assign(X);
				divisor += 2;
				term.Divide(divisor);
				if (term.IsZero())
					break;

				if (subtractTerm)
					this.Subtract(term);
				else
					this.Add(term);
				subtractTerm = !subtractTerm;
			}
		}
		public bool IsZero()
		{
			foreach (UInt32 item in number)
			{
				if (item != 0)
					return false;
			}
			return true;
		}

		public string Print()
		{
			BigNumber temp = new BigNumber(maxDigits);
			temp.Assign(this);

			StringBuilder sb = new StringBuilder();
			sb.Append(temp.number[0]);
			sb.Append(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator);

			int digitCount = 0;
			while (digitCount < maxDigits)
			{
				temp.number[0] = 0;
				temp.Multiply(100000);
				sb.AppendFormat("{0:D5}", temp.number[0]);
				digitCount += 5;
			}

			return sb.ToString();
		}
	}

	public partial class _Default : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var numdigitsarg = Request["numdigits"];
			if (string.IsNullOrEmpty(numdigitsarg))
				numdigitsarg = "5000";
			var numdigits = int.Parse(numdigitsarg);
			var start = DateTime.Now;
			BigNumber x = new BigNumber(numdigits);
			BigNumber y = new BigNumber(numdigits);
			x.ArcTan(16, 5);
			y.ArcTan(4, 239);
			x.Subtract(y);
			var done = DateTime.Now;
			Literal1.Text = x.Print();
			Literal2.Text = string.Format("{0} - {1}", (done - start).ToString(), System.Environment.MachineName);
		}

	}
}