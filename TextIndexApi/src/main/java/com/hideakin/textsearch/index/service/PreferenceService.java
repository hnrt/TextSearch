package com.hideakin.textsearch.index.service;

public interface PreferenceService {

	String getPreference(String name);
	void createPreference(String name, String value);
	boolean updatePreference(String name, String value);
	boolean deletePreference(String name);
	boolean isServiceUnavailable();
	void setServiceAvailability(boolean value);

}
