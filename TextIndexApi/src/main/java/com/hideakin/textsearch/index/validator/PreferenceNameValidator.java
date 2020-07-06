package com.hideakin.textsearch.index.validator;

public class PreferenceNameValidator {

	public static boolean isValid(String name) {
		if (name == null || name.length() == 0) {
			return false;
		}
		if (name.startsWith(" ") || name.endsWith(" ")) {
			return false;
		}
		if (Character.isDigit(name.charAt(0))) {
			return false;
		}
		if (name.contains("/")) {
			return false;
		}
		return true;
	}

}
