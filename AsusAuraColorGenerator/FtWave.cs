using System;
using System.Collections.Generic;
using System.Linq;

namespace AsusAuraColorGenerator
{
	// Token: 0x02000006 RID: 6
	internal class FtWave
	{
		// Token: 0x0600002A RID: 42 RVA: 0x000032C6 File Offset: 0x000014C6
		public FtWave(double min, double max, double length, double freq, double phase)
		{
			this.m_range = new FtRange(min, max);
			this.m_min = min;
			this.m_max = max;
			this.m_length = length;
			this.m_freq = freq;
			this.m_phase = phase;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003300 File Offset: 0x00001500
		public FtWave(FtRange range, double length, double freq, double phase)
		{
			this.m_range = range;
			this.m_max = this.m_range.max;
			this.m_min = this.m_range.min;
			this.m_length = length;
			this.m_freq = freq;
			this.m_phase = phase;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003352 File Offset: 0x00001552
		private double getFrequency()
		{
			return this.m_freq;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x0000335A File Offset: 0x0000155A
		private double getWaveLength()
		{
			return this.m_length;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003362 File Offset: 0x00001562
		private double getPhase()
		{
			return this.m_phase;
		}

		// Token: 0x0600002F RID: 47 RVA: 0x0000336A File Offset: 0x0000156A
		private double getMin()
		{
			return this.m_min;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003372 File Offset: 0x00001572
		private double getMax()
		{
			return this.m_max;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x0000337A File Offset: 0x0000157A
		private void setFrequency(double freq)
		{
			this.m_freq = freq;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00003383 File Offset: 0x00001583
		private void setWaveLength(double length)
		{
			this.m_length = length;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x0000338C File Offset: 0x0000158C
		private void setPhase(double phase)
		{
			this.m_phase = phase;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00003395 File Offset: 0x00001595
		private void setMin(double min)
		{
			this.m_min = min;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x0000339E File Offset: 0x0000159E
		private void setMax(double max)
		{
			this.m_max = max;
		}

		// Token: 0x06000036 RID: 54 RVA: 0x000033A7 File Offset: 0x000015A7
		public virtual double calculatePhase(double t, double x)
		{
			return 0.0;
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000033B2 File Offset: 0x000015B2
		public virtual double calculate(double t, double x, double phase)
		{
			return 0.0;
		}

		// Token: 0x06000038 RID: 56 RVA: 0x000033C0 File Offset: 0x000015C0
		public double output(double t, double x)
		{
			double phase = this.calculatePhase(t, x);
			return this.calculate(t, x, phase);
		}

		// Token: 0x04000020 RID: 32
		private double m_min;

		// Token: 0x04000021 RID: 33
		private double m_max;

		// Token: 0x04000022 RID: 34
		private FtRange m_range;

		// Token: 0x04000023 RID: 35
		private double m_length;

		// Token: 0x04000024 RID: 36
		private double m_freq;

		// Token: 0x04000025 RID: 37
		private double m_phase;

		// Token: 0x04000026 RID: 38
		private double m_axis_length;

		// Token: 0x04000027 RID: 39
		private int m_compensate;

		// Token: 0x02000022 RID: 34
		public class CustomNode
		{
			// Token: 0x0600029A RID: 666 RVA: 0x00016118 File Offset: 0x00014318
			public CustomNode(double p, double f)
			{
				this.phase = p;
				this.fx = f;
			}

			// Token: 0x0400012C RID: 300
			public double phase;

			// Token: 0x0400012D RID: 301
			public double fx;
		}

		// Token: 0x02000023 RID: 35
		public class FtSineWave : FtWave
		{
			// Token: 0x0600029B RID: 667 RVA: 0x0001612E File Offset: 0x0001432E
			public FtSineWave(double min, double max, double length, double freq, double phase) : base(min, max, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x0600029C RID: 668 RVA: 0x0001615D File Offset: 0x0001435D
			public FtSineWave(FtRange range, double length, double freq, double phase) : base(range, length, freq, phase)
			{
				this.m_disAngular = -this.m_phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x0600029D RID: 669 RVA: 0x00016190 File Offset: 0x00014390
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 2.0 * 3.1415926535897931 / this.m_length - this.m_disAngular;
			}

			// Token: 0x0600029E RID: 670 RVA: 0x0001623B File Offset: 0x0001443B
			public override double calculate(double t, double x, double phase)
			{
				return (-Math.Cos(phase) + 1.0) * (this.m_max - this.m_min) / 2.0 + this.m_min;
			}

			// Token: 0x0400012E RID: 302
			private double m_disAngular;

			// Token: 0x0400012F RID: 303
			private double m_prevPhase;

			// Token: 0x04000130 RID: 304
			private double m_t;
		}

		// Token: 0x02000024 RID: 36
		public class FtHalfSineWave : FtWave
		{
			// Token: 0x0600029F RID: 671 RVA: 0x0001626D File Offset: 0x0001446D
			public FtHalfSineWave(double min, double max, double length, double freq, double phase) : base(min, max, length, freq, phase)
			{
				this.m_disAngular = -this.m_phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002A0 RID: 672 RVA: 0x000162A0 File Offset: 0x000144A0
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				double num4 = x * 2.0 * 3.1415926535897931 / this.m_length - this.m_disAngular;
				int num5 = (int)Math.Floor(num4 / 3.1415926535897931);
				return num4 - (double)num5 * 3.1415926535897931;
			}

			// Token: 0x060002A1 RID: 673 RVA: 0x0001636A File Offset: 0x0001456A
			public override double calculate(double t, double x, double phase)
			{
				return Math.Abs(Math.Sin(phase)) * (this.m_max - this.m_min) + this.m_min;
			}

			// Token: 0x04000131 RID: 305
			private double m_disAngular;

			// Token: 0x04000132 RID: 306
			private double m_prevPhase;

			// Token: 0x04000133 RID: 307
			private double m_t;
		}

		// Token: 0x02000025 RID: 37
		public class FtQuarterSineWave : FtWave
		{
			// Token: 0x060002A2 RID: 674 RVA: 0x0001638C File Offset: 0x0001458C
			public FtQuarterSineWave(double min, double max, double length, double freq, double phase) : base(min, max, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002A3 RID: 675 RVA: 0x000163BC File Offset: 0x000145BC
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				double num4 = x * 2.0 * 3.1415926535897931 / this.m_length - this.m_disAngular;
				int num5 = (int)Math.Floor(num4 / 1.5707963267948966);
				return num4 - (double)num5 * 1.5707963267948966;
			}

			// Token: 0x060002A4 RID: 676 RVA: 0x00016486 File Offset: 0x00014686
			public override double calculate(double t, double x, double phase)
			{
				return Math.Abs(Math.Sin(phase)) * (this.m_max - this.m_min) + this.m_min;
			}

			// Token: 0x04000134 RID: 308
			private double m_disAngular;

			// Token: 0x04000135 RID: 309
			private double m_prevPhase;

			// Token: 0x04000136 RID: 310
			private double m_t;
		}

		// Token: 0x02000026 RID: 38
		public class FtSquareWave : FtWave
		{
			// Token: 0x060002A5 RID: 677 RVA: 0x000164A8 File Offset: 0x000146A8
			private void setDuty(double duty)
			{
				this.m_duty = duty;
			}

			// Token: 0x060002A6 RID: 678 RVA: 0x000164B1 File Offset: 0x000146B1
			public FtSquareWave(double min, double max, double length, double freq, double phase) : base(min, max, length, freq, phase)
			{
				this.m_distance = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
				this.m_duty = length / 2.0;
			}

			// Token: 0x060002A7 RID: 679 RVA: 0x000164F1 File Offset: 0x000146F1
			public FtSquareWave(double min, double max, double length, double freq, double phase, double duty) : base(min, max, length, freq, phase)
			{
				this.m_distance = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
				this.m_duty = duty;
			}

			// Token: 0x060002A8 RID: 680 RVA: 0x00016528 File Offset: 0x00014728
			public FtSquareWave(FtRange range, double length, double freq, double phase) : base(range, length, freq, phase)
			{
				this.m_distance = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
				this.m_duty = length / 2.0;
			}

			// Token: 0x060002A9 RID: 681 RVA: 0x00016566 File Offset: 0x00014766
			public FtSquareWave(FtRange range, double length, double freq, double phase, double duty) : base(range, length, freq, phase)
			{
				this.m_distance = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
				this.m_duty = duty;
			}

			// Token: 0x060002AA RID: 682 RVA: 0x0001659C File Offset: 0x0001479C
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = num * this.m_length * this.m_freq;
					this.m_distance += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_distance -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x - this.m_distance;
			}

			// Token: 0x060002AB RID: 683 RVA: 0x00016620 File Offset: 0x00014820
			public override double calculate(double t, double x, double phase)
			{
				double num = phase % this.m_length / this.m_length;
				double num2 = 1E-05;
				if (num <= -num2)
				{
					num += this.m_length;
				}
				if (num < this.m_length - this.m_duty)
				{
					return this.m_min;
				}
				return this.m_max;
			}

			// Token: 0x04000137 RID: 311
			private double m_distance;

			// Token: 0x04000138 RID: 312
			private double m_prevPhase;

			// Token: 0x04000139 RID: 313
			private double m_t;

			// Token: 0x0400013A RID: 314
			private double m_duty;
		}

		// Token: 0x02000027 RID: 39
		public class FtTriangleWave : FtWave
		{
			// Token: 0x060002AC RID: 684 RVA: 0x00016672 File Offset: 0x00014872
			public FtTriangleWave(double min, double max, double length, double freq, double phase) : base(min, max, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002AD RID: 685 RVA: 0x000166A1 File Offset: 0x000148A1
			public FtTriangleWave(FtRange range, double length, double freq, double phase) : base(range, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002AE RID: 686 RVA: 0x000166D0 File Offset: 0x000148D0
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 2.0 * 3.1415926535897931 / this.m_length - this.m_disAngular;
			}

			// Token: 0x060002AF RID: 687 RVA: 0x0001677C File Offset: 0x0001497C
			public override double calculate(double t, double x, double phase)
			{
				return (Math.Asin(Math.Sin(phase)) * 2.0 / 3.1415926535897931 + 1.0) * (this.m_max - this.m_min) / 2.0 + this.m_min;
			}

			// Token: 0x0400013B RID: 315
			private double m_disAngular;

			// Token: 0x0400013C RID: 316
			private double m_prevPhase;

			// Token: 0x0400013D RID: 317
			private double m_t;
		}

		// Token: 0x02000028 RID: 40
		public class FtSawToothleWave : FtWave
		{
			// Token: 0x060002B0 RID: 688 RVA: 0x000167D1 File Offset: 0x000149D1
			public FtSawToothleWave(double min, double max, double length, double freq, double phase) : base(min, max, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002B1 RID: 689 RVA: 0x00016800 File Offset: 0x00014A00
			public FtSawToothleWave(FtRange range, double length, double freq, double phase) : base(range, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002B2 RID: 690 RVA: 0x00016830 File Offset: 0x00014A30
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 2.0 * 3.1415926535897931 / this.m_length - this.m_disAngular - 3.1415926535897931;
			}

			// Token: 0x060002B3 RID: 691 RVA: 0x000168E8 File Offset: 0x00014AE8
			public override double calculate(double t, double x, double phase)
			{
				return (Math.Atan(Math.Tan(phase / 2.0)) * 2.0 / 3.1415926535897931 + 1.0) * (this.m_max - this.m_min) / 2.0 + this.m_min;
			}

			// Token: 0x0400013E RID: 318
			private double m_disAngular;

			// Token: 0x0400013F RID: 319
			private double m_prevPhase;

			// Token: 0x04000140 RID: 320
			private double m_t;
		}

		// Token: 0x02000029 RID: 41
		public class FtLinearWave : FtWave
		{
			// Token: 0x060002B4 RID: 692 RVA: 0x00016948 File Offset: 0x00014B48
			public FtLinearWave(double min, double max, double length, double freq, double phase, List<FtWave.CustomNode> nodes) : base(min, max, length, freq / 2.0, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
				this.m_nodes = nodes;
			}

			// Token: 0x060002B5 RID: 693 RVA: 0x00016994 File Offset: 0x00014B94
			public FtLinearWave(FtRange range, double length, double freq, double phase) : base(range, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002B6 RID: 694 RVA: 0x000169C4 File Offset: 0x00014BC4
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 3.1415926535897931 / this.m_length - this.m_disAngular - 3.1415926535897931;
			}

			// Token: 0x060002B7 RID: 695 RVA: 0x00016A70 File Offset: 0x00014C70
			public override double calculate(double t, double x, double phase)
			{
				int num = 0;
				int num2 = 0;
				double num3 = Math.Abs(phase % 3.1415926535897931) / 3.1415926535897931;
				foreach (FtWave.CustomNode customNode in this.m_nodes)
				{
					if (num3 >= customNode.phase)
					{
						num = num2;
					}
					num2++;
				}
				this.m_nodes.Count<FtWave.CustomNode>();
				if (num == this.m_nodes.Count<FtWave.CustomNode>() - 1)
				{
					return this.m_nodes[num].fx;
				}
				double fx = this.m_nodes[num].fx;
				double phase2 = this.m_nodes[num].phase;
				double fx2 = this.m_nodes[num + 1].fx;
				double phase3 = this.m_nodes[num + 1].phase;
				return (fx2 - fx) * (num3 - phase2) / (phase3 - phase2) + fx;
			}

			// Token: 0x04000141 RID: 321
			private double m_disAngular;

			// Token: 0x04000142 RID: 322
			private double m_prevPhase;

			// Token: 0x04000143 RID: 323
			private double m_t;

			// Token: 0x04000144 RID: 324
			private List<FtWave.CustomNode> m_nodes;
		}

		// Token: 0x0200002A RID: 42
		public class FtSymmetricLinearWave : FtWave
		{
			// Token: 0x060002B8 RID: 696 RVA: 0x00016B78 File Offset: 0x00014D78
			public FtSymmetricLinearWave(double min, double max, double length, double freq, double phase, List<FtWave.CustomNode> nodes) : base(min, max, length, freq / 2.0, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
				this.m_nodes = nodes;
				for (int i = nodes.Count<FtWave.CustomNode>() - 2; i > 0; i--)
				{
					FtWave.CustomNode item = new FtWave.CustomNode(3.1415926535897931 + Math.Abs(3.1415926535897931 - this.m_nodes[i].phase), this.m_nodes[i].fx);
					this.m_nodes.Add(item);
				}
			}

			// Token: 0x060002B9 RID: 697 RVA: 0x00016C25 File Offset: 0x00014E25
			public FtSymmetricLinearWave(FtRange range, double length, double freq, double phase) : base(range, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002BA RID: 698 RVA: 0x00016C54 File Offset: 0x00014E54
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 3.1415926535897931 / this.m_length - this.m_disAngular - 3.1415926535897931;
			}

			// Token: 0x060002BB RID: 699 RVA: 0x00016D00 File Offset: 0x00014F00
			public override double calculate(double t, double x, double phase)
			{
				int num = 0;
				int num2 = 0;
				double num3 = Math.Abs(phase % 3.1415926535897931 / 3.1415926535897931 * 2.0);
				foreach (FtWave.CustomNode customNode in this.m_nodes)
				{
					if (num3 >= customNode.phase)
					{
						num = num2;
					}
					num2++;
				}
				this.m_nodes.Count<FtWave.CustomNode>();
				if (num == this.m_nodes.Count<FtWave.CustomNode>() - 1)
				{
					return this.m_nodes[num].fx;
				}
				double fx = this.m_nodes[num].fx;
				double phase2 = this.m_nodes[num].phase;
				double fx2 = this.m_nodes[num + 1].fx;
				double phase3 = this.m_nodes[num + 1].phase;
				if (fx2 != fx && phase3 != phase2)
				{
					return (fx2 - fx) * (num3 - phase2) / (phase3 - phase2) + fx;
				}
				return fx;
			}

			// Token: 0x04000145 RID: 325
			private double m_disAngular;

			// Token: 0x04000146 RID: 326
			private double m_prevPhase;

			// Token: 0x04000147 RID: 327
			private double m_t;

			// Token: 0x04000148 RID: 328
			private List<FtWave.CustomNode> m_nodes;
		}

		// Token: 0x0200002B RID: 43
		public class FtShortPathLinearWave : FtWave
		{
			// Token: 0x060002BC RID: 700 RVA: 0x00016E24 File Offset: 0x00015024
			public FtShortPathLinearWave(double min, double max, double length, double freq, double phase, List<FtWave.CustomNode> nodes) : base(min, max, length, freq / 2.0, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
				this.m_nodes = nodes;
			}

			// Token: 0x060002BD RID: 701 RVA: 0x00016E70 File Offset: 0x00015070
			public FtShortPathLinearWave(FtRange range, double length, double freq, double phase) : base(range, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002BE RID: 702 RVA: 0x00016EA0 File Offset: 0x000150A0
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 3.1415926535897931 / this.m_length - this.m_disAngular - 3.1415926535897931;
			}

			// Token: 0x060002BF RID: 703 RVA: 0x00016F4C File Offset: 0x0001514C
			public override double calculate(double t, double x, double phase)
			{
				int num = 0;
				int num2 = 0;
				double num3 = Math.Abs(phase % 3.1415926535897931 / 3.1415926535897931);
				foreach (FtWave.CustomNode customNode in this.m_nodes)
				{
					if (num3 >= customNode.phase)
					{
						num = num2;
					}
					num2++;
				}
				this.m_nodes.Count<FtWave.CustomNode>();
				if (num == this.m_nodes.Count<FtWave.CustomNode>() - 1)
				{
					return this.m_nodes[num].fx;
				}
				double num4 = this.m_nodes[num].fx * 360.0;
				double phase2 = this.m_nodes[num].phase;
				double num5 = this.m_nodes[num + 1].fx * 360.0;
				double phase3 = this.m_nodes[num + 1].phase;
				double num6;
				if ((num5 - num4 + 360.0) % 360.0 < 180.0)
				{
					if (num5 < 180.0 && num4 > 180.0)
					{
						num6 = (num5 + 360.0 - num4) * (num3 - phase2) / (phase3 - phase2) + num4;
					}
					else
					{
						num6 = (num5 - num4) * (num3 - phase2) / (phase3 - phase2) + num4;
					}
				}
				else if (num4 < 180.0 && num5 > 180.0)
				{
					num6 = (num4 + 360.0 - num5) * (num3 - phase3) / (phase2 - phase3) + num5;
				}
				else
				{
					num6 = (num4 - num5) * (num3 - phase3) / (phase2 - phase3) + num5;
				}
				if (num6 > 360.0)
				{
					num6 -= 360.0;
				}
				return num6 / 360.0;
			}

			// Token: 0x04000149 RID: 329
			private double m_disAngular;

			// Token: 0x0400014A RID: 330
			private double m_prevPhase;

			// Token: 0x0400014B RID: 331
			private double m_t;

			// Token: 0x0400014C RID: 332
			private List<FtWave.CustomNode> m_nodes;
		}

		// Token: 0x0200002C RID: 44
		public class RandomDoubleGenerator
		{
			// Token: 0x060002C0 RID: 704 RVA: 0x00017140 File Offset: 0x00015340
			public RandomDoubleGenerator(double max, double min)
			{
				this.m_max = max;
				this.m_min = min;
				this.m_rand = new Random();
			}

			// Token: 0x060002C1 RID: 705 RVA: 0x00017164 File Offset: 0x00015364
			public double Next()
			{
				double num;
				do
				{
					num = this.m_rand.NextDouble();
				}
				while (num > this.m_max || num < this.m_min);
				return num;
			}

			// Token: 0x0400014D RID: 333
			private double m_max;

			// Token: 0x0400014E RID: 334
			private double m_min;

			// Token: 0x0400014F RID: 335
			private Random m_rand;
		}

		// Token: 0x0200002D RID: 45
		public class FtRandomWave : FtWave
		{
			// Token: 0x060002C2 RID: 706 RVA: 0x00017190 File Offset: 0x00015390
			public FtRandomWave(double min, double max, double length, double freq) : base(min, max, length, freq, 0.0)
			{
				this.m_disAngular = 0.0;
				this.m_t = 0.0;
				this.m_prevPhase = 0.0;
				this.m_rand = new FtWave.RandomDoubleGenerator(max, min);
				this.m_diced = false;
				this.m_value = this.m_rand.Next();
			}

			// Token: 0x060002C3 RID: 707 RVA: 0x00017204 File Offset: 0x00015404
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 2.0 * 3.1415926535897931 / this.m_length - this.m_disAngular;
			}

			// Token: 0x060002C4 RID: 708 RVA: 0x000172B0 File Offset: 0x000154B0
			public override double calculate(double t, double x, double phase)
			{
				double num = Math.Abs(phase % 6.2831853071795862);
				if (this.m_diced && num > 3.1415926535897931)
				{
					this.m_diced = false;
				}
				else if (!this.m_diced && num < 3.1415926535897931)
				{
					this.m_value = this.m_rand.Next();
					this.m_diced = true;
				}
				return this.m_value;
			}

			// Token: 0x04000150 RID: 336
			private double m_disAngular;

			// Token: 0x04000151 RID: 337
			private double m_prevPhase;

			// Token: 0x04000152 RID: 338
			private double m_t;

			// Token: 0x04000153 RID: 339
			private double m_value;

			// Token: 0x04000154 RID: 340
			private bool m_diced;

			// Token: 0x04000155 RID: 341
			private FtWave.RandomDoubleGenerator m_rand;
		}

		// Token: 0x0200002E RID: 46
		public class FtStepWave : FtWave
		{
			// Token: 0x060002C5 RID: 709 RVA: 0x00017320 File Offset: 0x00015520
			public FtStepWave(double min, double max, double length, double freq, double phase, List<FtWave.CustomNode> nodes) : base(min, max, length, freq / 2.0, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
				this.m_nodes = nodes;
			}

			// Token: 0x060002C6 RID: 710 RVA: 0x0001736C File Offset: 0x0001556C
			public FtStepWave(FtRange range, double length, double freq, double phase) : base(range, length, freq, phase)
			{
				this.m_disAngular = -phase;
				this.m_t = 0.0;
				this.m_prevPhase = phase;
			}

			// Token: 0x060002C7 RID: 711 RVA: 0x0001739C File Offset: 0x0001559C
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 3.1415926535897931 / this.m_length - this.m_disAngular - 3.1415926535897931;
			}

			// Token: 0x060002C8 RID: 712 RVA: 0x00017448 File Offset: 0x00015648
			public override double calculate(double t, double x, double phase)
			{
				int index = 0;
				int num = 0;
				double num2 = Math.Abs(phase % 3.1415926535897931 / 3.1415926535897931);
				foreach (FtWave.CustomNode customNode in this.m_nodes)
				{
					if (num2 >= customNode.phase)
					{
						index = num;
					}
					num++;
				}
				return this.m_nodes[index].fx;
			}

			// Token: 0x04000156 RID: 342
			private double m_disAngular;

			// Token: 0x04000157 RID: 343
			private double m_prevPhase;

			// Token: 0x04000158 RID: 344
			private double m_t;

			// Token: 0x04000159 RID: 345
			private List<FtWave.CustomNode> m_nodes;
		}

		// Token: 0x0200002F RID: 47
		public class FtConstantWave : FtWave
		{
			// Token: 0x060002C9 RID: 713 RVA: 0x000174D8 File Offset: 0x000156D8
			public FtConstantWave(double max, double length) : base(0.0, max, length, 0.0, 0.0)
			{
				this.m_disAngular = 0.0;
				this.m_t = 0.0;
				this.m_prevPhase = 0.0;
			}

			// Token: 0x060002CA RID: 714 RVA: 0x00017538 File Offset: 0x00015738
			public override double calculatePhase(double t, double x)
			{
				double num = t - this.m_t;
				double num2 = this.m_phase - this.m_prevPhase;
				if (num != 0.0)
				{
					double num3 = this.m_freq * num * 2.0 * 3.1415926535897931;
					this.m_disAngular += num3;
					this.m_t = t;
				}
				if (num2 != 0.0)
				{
					this.m_disAngular -= num2;
					this.m_prevPhase = this.m_phase;
				}
				return x * 2.0 * 3.1415926535897931 / this.m_length - this.m_disAngular - 3.1415926535897931;
			}

			// Token: 0x060002CB RID: 715 RVA: 0x000175ED File Offset: 0x000157ED
			public override double calculate(double t, double x, double phase)
			{
				return this.m_max;
			}

			// Token: 0x0400015A RID: 346
			private double m_disAngular;

			// Token: 0x0400015B RID: 347
			private double m_prevPhase;

			// Token: 0x0400015C RID: 348
			private double m_t;

			// Token: 0x0400015D RID: 349
			private List<FtWave.CustomNode> m_nodes;
		}

		// Token: 0x02000030 RID: 48
		public class FtFixedPhaseSineWave : FtWave.FtSineWave
		{
			// Token: 0x060002CC RID: 716 RVA: 0x000175F5 File Offset: 0x000157F5
			public FtFixedPhaseSineWave(double min, double max, double length, double freq, double phase, double minPhase, double maxPhase) : base(min, max, length, freq, phase)
			{
				this.m_minPhase = minPhase;
				this.m_maxPhase = maxPhase;
			}

			// Token: 0x060002CD RID: 717 RVA: 0x00017614 File Offset: 0x00015814
			public FtFixedPhaseSineWave(FtRange range, double length, double freq, double phase, double minPhase, double maxPhase) : base(range, length, freq, phase)
			{
				this.m_minPhase = minPhase;
				this.m_maxPhase = maxPhase;
			}

			// Token: 0x060002CE RID: 718 RVA: 0x00017634 File Offset: 0x00015834
			public override double calculatePhase(double t, double x)
			{
				double num = base.calculatePhase(t, x);
				if (num > this.m_maxPhase)
				{
					num = this.m_maxPhase;
				}
				if (num < this.m_minPhase)
				{
					num = this.m_minPhase;
				}
				return num;
			}

			// Token: 0x0400015E RID: 350
			private double m_minPhase;

			// Token: 0x0400015F RID: 351
			private double m_maxPhase;
		}

		// Token: 0x02000031 RID: 49
		public class FtStretchSineWave : FtWave
		{
			// Token: 0x060002CF RID: 719 RVA: 0x0001766C File Offset: 0x0001586C
			public FtStretchSineWave(double min, double max, double wavelength, double start, double freq) : base(min, max, wavelength, freq, 0.0)
			{
				this.m_disAngular = 0.0;
				this.m_t = 0.0;
				this.m_prevPhase = 0.0;
				this.m_stretch = wavelength;
				this.m_start = min;
				this.m_f = new FtWave.FtSineWave(-this.m_filter, wavelength + this.m_filter, 22.0, freq, 0.0);
			}

			// Token: 0x060002D0 RID: 720 RVA: 0x000176F6 File Offset: 0x000158F6
			public override double calculatePhase(double t, double x)
			{
				return 0.0;
			}

			// Token: 0x060002D1 RID: 721 RVA: 0x00017701 File Offset: 0x00015901
			public override double calculate(double t, double x, double phase)
			{
				this.m_stretch = this.m_f.output(t, 0.0);
				if (x > this.m_stretch || x < this.m_start)
				{
					return this.m_min;
				}
				return this.m_max;
			}

			// Token: 0x04000160 RID: 352
			private double m_disAngular;

			// Token: 0x04000161 RID: 353
			private double m_prevPhase;

			// Token: 0x04000162 RID: 354
			private double m_t;

			// Token: 0x04000163 RID: 355
			private double m_stretch;

			// Token: 0x04000164 RID: 356
			private double m_start;

			// Token: 0x04000165 RID: 357
			private FtWave.FtSineWave m_f;

			// Token: 0x04000166 RID: 358
			private List<FtWave.CustomNode> m_nodes;

			// Token: 0x04000167 RID: 359
			private double m_filter;
		}
	}
}
