package com.hideakin.textsearch.index.utility;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.context.SpringBootTest;

@SpringBootTest
public class DistributionCoderTests {
	
	public static final int START1 = 0;
	public static final int START2 = 128;
	public static final int START3 = 2048;
	public static final int START4 = 65536;
	public static final int START5 = 2097152;
	public static final int START6 = 67108864;

	@Test
	public void test1() throws Exception {
		testByRange(START1, START2 - 1);
	}

	@Test
	public void test2() throws Exception {
		testByRange(START2, START3 - 1);
	}

	@Test
	public void test3() throws Exception {
		testByRange(START3, START4 - 1);
	}

	@Test
	public void test4() throws Exception {
		testByRange(START4, START5 - 1);
	}

	@Test
	public void test5() throws Exception {
		testByRange(START5, START6 - 1);
	}

	@Test
	public void test6() throws Exception {
		testByRange(START6, Integer.MAX_VALUE);
	}

	private void testByRange(long start, long end) throws Exception {
		int count = 0;
		int[] data = new int[65536];
		for (long value = start; value <= end; value++) {
			data[count++] = (int)value;
			if (count == data.length) {
				doit(data, count);
				count = 0;
			}
		}
		if (count > 0) {
			doit(data, count);
		}
	}

	private void doit(int[] data, int count) throws Exception {
		DistributionEncoder encoder = new DistributionEncoder();
		for (int index = 0; index < count; index++) {
			encoder.write(data[index]);
		}
		DistributionDecoder decoder = new DistributionDecoder(encoder.flush());
		int decoded = decoder.read();
		for (int index = 0; index < count; index++) {
			Assertions.assertEquals(data[index], decoded);
			decoded = decoder.read();
		}
		Assertions.assertTrue(decoded < 0);
	}
}
