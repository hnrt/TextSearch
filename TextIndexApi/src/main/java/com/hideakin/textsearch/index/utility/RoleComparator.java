package com.hideakin.textsearch.index.utility;

import java.util.Comparator;

public class RoleComparator implements Comparator<String> {

	@Override
	public int compare(String arg0, String arg1) {
		return arg0.compareToIgnoreCase(arg1);
	}
	
}
