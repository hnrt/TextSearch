package com.hideakin.textsearch.index.validator;

public class FileGroupNameValidator {

	public static final int GROUPNAME_MAXLEN = 255;

	private static final String INVALID_CHARS = "/?&,";
	
	public static boolean isValid(String name) {
		if (name == null || name.length() == 0 || name.length() > GROUPNAME_MAXLEN) {
			return false;
		}
		char first = name.charAt(0);
		if (Character.isWhitespace(first) || Character.isDigit(first)) {
			return false;
		}
		char last = name.charAt(name.length() - 1);
		if (Character.isWhitespace(last)) {
			return false;
		}
		for (char c : name.toCharArray()) {
			if (INVALID_CHARS.indexOf(c) >= 0) {
				return false;
			}
		}
		return true;
	}

}
