package com.hideakin.textsearch.index.service;

public interface PreferenceService {

	String getPreference(String name);
	boolean setPreference(String name, String value);
	boolean resetPreference(String original, String name, String value);
	boolean deletePreference(String name);
	boolean isServiceUnavailable();
	void setServiceAvailability(boolean value);

}
