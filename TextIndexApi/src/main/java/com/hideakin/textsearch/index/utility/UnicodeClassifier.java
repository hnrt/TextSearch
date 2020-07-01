package com.hideakin.textsearch.index.utility;

public class UnicodeClassifier {

	public static boolean isHiragana(int c) {
		return 0x3040 <= c && c <= 0x309F;
	}
	
	public static boolean isKatakana(int c) {
		return 0x30A0 <= c && c <= 0x30FF;
	}
	
	public static boolean isKatakanaPhoneticExtensions(int c) {
		return 0x31F0 <= c && c <= 0x31FF;
	}
	
	public static boolean isIdeograph(int c) {
		return 0x4E00 <= c && c <= 0x9FFC;
	}
	
	public static boolean isHalfwidthKatakana(int c) {
		return 0xFF66 <= c && c <= 0xFF9F;
	}
	
	public static boolean isJapaneseLetter(int c) {
		return isHiragana(c) || isKatakana(c) || isKatakanaPhoneticExtensions(c) || isIdeograph(c) || isHalfwidthKatakana(c);
	}
	
	public static boolean isFullwidthDigit(int c) {
		return 0xFF10 <= c && c <= 0xFF19;
	}
	
	public static boolean isFullwidthUppercaseAlphabet(int c) {
		return 0xFF21 <= c && c <= 0xFF3A;
	}
	
	public final int FULLWIDTH_COMMERCIAL_AT = 0xFF20;
	
	public static boolean isFullwidthLowercaseAlphabet(int c) {
		return 0xFF41 <= c && c <= 0xFF5A;
	}
	
	public static boolean isFullwidthAlphabet(int c) {
		return isFullwidthUppercaseAlphabet(c) || isFullwidthLowercaseAlphabet(c);
	}
	
	public static boolean isFullwidthAlphaNumeric(int c) {
		return isFullwidthAlphabet(c) || isFullwidthDigit(c);
	}

}
