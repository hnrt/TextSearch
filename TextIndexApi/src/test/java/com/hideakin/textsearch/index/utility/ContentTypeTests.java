package com.hideakin.textsearch.index.utility;

import java.nio.charset.Charset;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.context.SpringBootTest;

@SpringBootTest
public class ContentTypeTests {

	@Test
	public void test_null() {
		ContentType ct = ContentType.parse(null);
		Assertions.assertEquals(null, ct.getMimeType());
		Assertions.assertEquals(null, ct.getMimeSubtype());
		Assertions.assertEquals(null, ct.getParameter("charset"));
	}

	@Test
	public void test_empty() {
		ContentType ct = ContentType.parse("");
		Assertions.assertEquals(null, ct.getMimeType());
		Assertions.assertEquals(null, ct.getMimeSubtype());
		Assertions.assertEquals(null, ct.getParameter("charset"));
	}

	@Test
	public void test_typeOnly() {
		ContentType ct = ContentType.parse("text");
		Assertions.assertEquals("text", ct.getMimeType());
		Assertions.assertEquals(null, ct.getMimeSubtype());
		Assertions.assertEquals(null, ct.getParameter("charset"));
	}

	@Test
	public void test_typeAndSubtype() {
		ContentType ct = ContentType.parse("text/plain");
		Assertions.assertEquals("text", ct.getMimeType());
		Assertions.assertEquals("plain", ct.getMimeSubtype());
		Assertions.assertEquals(null, ct.getParameter("charset"));
	}

	@Test
	public void test_typeAndSubtypeWithWS() {
		ContentType ct = ContentType.parse(" text  / plain  ");
		Assertions.assertEquals("text", ct.getMimeType());
		Assertions.assertEquals("plain", ct.getMimeSubtype());
		Assertions.assertEquals(null, ct.getParameter("charset"));
		Charset cs1 = ct.getCharset("UTF-16");
		Assertions.assertNotEquals(null, cs1);
		Assertions.assertEquals("UTF-16", cs1.displayName());
		Charset cs2 = ct.getCharset(null);
		Assertions.assertEquals(null, cs2);
	}

	@Test
	public void test_typeSubtypeAndCharset() {
		ContentType ct = ContentType.parse("text/plain;charset=utf-8");
		Assertions.assertEquals("text", ct.getMimeType());
		Assertions.assertEquals("plain", ct.getMimeSubtype());
		Assertions.assertEquals("utf-8", ct.getParameter("charset"));
		Charset cs = ct.getCharset("UTF-16");
		Assertions.assertNotEquals(null, cs);
		Assertions.assertEquals("UTF-8", cs.displayName());
	}

	@Test
	public void test_typeSubtypeAndCharsetWithWS() {
		ContentType ct = ContentType.parse("text /   plain  ; charset  =   utf-8   ");
		Assertions.assertEquals("text", ct.getMimeType());
		Assertions.assertEquals("plain", ct.getMimeSubtype());
		Assertions.assertEquals("utf-8", ct.getParameter("charset"));
	}

	@Test
	public void test_typeSubtypeAndQuotedCharsetWithWS() {
		ContentType ct = ContentType.parse("text /   plain  ; charset  =   \"utf\\-8\" ; bogus = xyzzy  ");
		Assertions.assertEquals("text", ct.getMimeType());
		Assertions.assertEquals("plain", ct.getMimeSubtype());
		Assertions.assertEquals("utf-8", ct.getParameter("charSet"));
		Assertions.assertEquals("xyzzy", ct.getParameter("Bogus"));
	}

}
