package com.hideakin.textsearch.index.validator;

public class UserNameValidator {

	public static final int USERNAME_MAXLEN = 255;

	private static final String INVALID_CHARS = "/?&,";

	public static final int ROLES_MAXLEN = 255;

	public static boolean isValidUsername(String name) {
		if (name == null || name.length() == 0 || name.length() > USERNAME_MAXLEN) {
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

	public static boolean areValidRoles(String[] roles) {
		int csvLength = 0;
		for (String role : roles) {
			if (!isValidUsername(role)) {
				return false;
			}
			csvLength += 1 + role.length();
		}
		if (csvLength > 1 + ROLES_MAXLEN) {
			return false;
		}
		return true;
	}

}
