package com.hideakin.textsearch.index.utility;

public class RolesIntersection {

	// It is assumed that both roles1 and roles2 have been sorted in the dictionary order 
	public static boolean Exists(String[] roles1, String[] roles2) {
		int index1 = 0;
		int index2 = 0;
		while (index1 < roles1.length && index2 < roles2.length) {
			int result = roles1[index1].compareToIgnoreCase(roles2[index2]);
			if (result == 0) {
				return true;
			} else if (result < 0) {
				index1++;
			} else {
				index2++;
			}
		}
		return false;
	}

}
