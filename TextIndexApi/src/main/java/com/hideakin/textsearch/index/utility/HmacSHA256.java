package com.hideakin.textsearch.index.utility;

import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;

public class HmacSHA256 {

	public static final String HMAC_ALGO = "HmacSHA256";
	public static final Charset HMAC_CSET = StandardCharsets.UTF_8;

	public static String compute(String input, String key) {
		try {
			SecretKeySpec sks = new SecretKeySpec(key.getBytes(HMAC_CSET), HMAC_ALGO);
			Mac mac = Mac.getInstance(HMAC_ALGO);
			mac.init(sks);
			byte[] digest = mac.doFinal(input.getBytes(HMAC_CSET));
			StringBuilder obuf = new StringBuilder(2 * digest.length);
			for (byte d : digest) {
				obuf.append(String.format("%02X", d & 0xff) );
			}
			return obuf.toString();
		} catch (NoSuchAlgorithmException e) {
			e.printStackTrace();
		} catch (InvalidKeyException e) {
			e.printStackTrace();
		}
		return null;
	}

}
