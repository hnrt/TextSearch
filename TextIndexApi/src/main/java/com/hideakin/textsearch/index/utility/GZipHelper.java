package com.hideakin.textsearch.index.utility;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.util.zip.GZIPInputStream;
import java.util.zip.GZIPOutputStream;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class GZipHelper {

	private static final Logger logger = LoggerFactory.getLogger(GZipHelper.class);

	public static byte[] compress(byte[] data) {
		ByteArrayOutputStream outstr = new ByteArrayOutputStream();
		try (OutputStream gzip = new GZIPOutputStream(outstr)) {
	        gzip.write(data);
		} catch (IOException e) {
			e.printStackTrace();
		}
        return outstr.toByteArray();
	}

	public static byte[] decompress(byte[] data, int size) {
		try (InputStream gzip = new GZIPInputStream(new ByteArrayInputStream(data))) {
	    	byte[] buf = new byte[size];
	    	int off = 0;
	    	while (off < size) {
		    	int len = gzip.read(buf, off, size - off);
	    		if (len <= 0) {
	    			// shorter than the expected
	    			logger.warn("decompress: expect={} actual={}", size, off);
	    			byte[] buf2 = new byte[off];
	    			System.arraycopy(buf, 0, buf2, 0, off);
	    			return buf2;
	    		}
	    		off += len;
	    	}
			int b = gzip.read();
			if (b > -1) {
				// longer than the expected
				ByteArrayOutputStream outstr = new ByteArrayOutputStream();
				outstr.write(buf, 0, off);
				outstr.write(b);
				while ((b = gzip.read()) > -1) {
	    			outstr.write(b);
				}
		        buf = outstr.toByteArray();
    			logger.warn("decompress: expect={} actual={}", size, buf.length);
			}
			return buf;
		} catch (IOException e) {
			e.printStackTrace();
			return null;
		}
	}

}
