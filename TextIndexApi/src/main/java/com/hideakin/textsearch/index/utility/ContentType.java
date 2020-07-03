package com.hideakin.textsearch.index.utility;

import java.io.IOException;
import java.io.StringReader;
import java.nio.charset.Charset;

import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;

public class ContentType {

	private String mimeType;
	
	private String mimeSubtype;
	
	private MultiValueMap<String,String> parameterMap;

	public ContentType() {
		mimeType = null;
		mimeSubtype = null;
		parameterMap = new LinkedMultiValueMap<>();
	}

	public String getMimeType() {
		return mimeType;
	}

	public String getMimeSubtype() {
		return mimeSubtype;
	}
	
	public Charset getCharset(String defaultCharsetName) {
		String name = parameterMap.getFirst("charset");
		return name != null ? Charset.forName(name) :
			defaultCharsetName != null ? Charset.forName(defaultCharsetName) : null;
	}

	public String getParameter(String name) {
		return parameterMap.getFirst(name.toLowerCase());
	}

	public static ContentType parse(String value) {
		ContentType ct = new ContentType();
		if (value == null || value.length() == 0) {
			return ct;
		}
		try {
			StringBuilder sb = new StringBuilder();
			StringReader in = new StringReader(value);
			int c = in.read();
			c = parseWS(c, in);
			if (c == -1) {
				return ct;
			}
			c = parseToken(c, in, sb);
			ct.mimeType = sb.toString();
			c = parseWS(c, in);
			if (c == '/') {
				c = in.read();
			} else {
				return ct;
			}
			c = parseWS(c, in);
			if (c == -1) {
				return ct;
			}
			c = parseToken(c, in, sb);
			ct.mimeSubtype = sb.toString();
			c = parseWS(c, in);
			while (c == ';') {
				c = in.read();
				c = parseWS(c, in);
				if (c == -1) {
					return ct;
				}
				c = parseToken(c, in, sb);
				String pname = sb.toString();
				c = parseWS(c, in);
				if (c == '=') {
					c = in.read();
				} else {
					return ct;
				}
				c = parseWS(c, in);
				if (c == '\"') {
					c = parseQuotedString(c, in, sb);
				} else {
					c = parseToken(c, in, sb);
				}
				String pvalue = sb.toString();
				ct.parameterMap.add(pname.toLowerCase(), pvalue);
				c = parseWS(c, in);
			}
		} catch (IOException e) {
			e.printStackTrace();
		}
		return ct;
	}
	
	public static int parseWS(int c, StringReader in) throws IOException {
		while (c != -1) {
			if (Character.isWhitespace(c)) {
				c = in.read();
			} else {
				return c;
			}
		}
		return -1;
	}

	public static int parseToken(int c, StringReader in, StringBuilder sb) throws IOException {
		sb.setLength(0);
		while (c != -1) {
			if (Character.isWhitespace(c) || c == '/' || c == ';' || c == '=') {
				return c;
			} else {
				sb.append((char)c);
				c = in.read();
			}
		}
		return -1;
	}

	public static int parseQuotedString(int c, StringReader in, StringBuilder sb) throws IOException {
		sb.setLength(0);
		c = in.read();
		while (c != -1) {
			if (c == '\"') {
				return in.read();
			} else if (c == '\\') {
				c = in.read();
				if (c == -1) {
					return -1;
				}
				sb.append((char)c);
				c = in.read();
			} else {
				sb.append((char)c);
				c = in.read();
			}
		}
		return -1;
	}

}
