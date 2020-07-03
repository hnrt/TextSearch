package com.hideakin.textsearch.index.utility;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.nio.charset.Charset;

public class TextEncoding {

	public static byte[] convert(byte[] data, Charset csIn, Charset csOut) {
		ByteArrayOutputStream outstr = new ByteArrayOutputStream();
		try (BufferedReader br = new BufferedReader(new InputStreamReader(new ByteArrayInputStream(data), csIn));
		     BufferedWriter bw = new BufferedWriter(new OutputStreamWriter(outstr, csOut))) {
			int c;
			while ((c = br.read()) != -1) {
				bw.write(c);
			}
		} catch (IOException e) {
			e.printStackTrace();
		}
		return outstr.toByteArray();
	}

}
