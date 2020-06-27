package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.UpdatePreferenceRequest;

public interface PreferenceService {

	String getPreference(String name);
	void updatePreference(UpdatePreferenceRequest req);
	void deletePreference(String name);
	boolean isServiceUnavailable();
	void setServiceAvailability(boolean value);

}
