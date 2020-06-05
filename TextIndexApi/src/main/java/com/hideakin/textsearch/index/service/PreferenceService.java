package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.UpdatePreferencesRequest;
import com.hideakin.textsearch.index.model.ValueResponse;

public interface PreferenceService {

	ValueResponse getPreference(String name);
	void updatePreferences(UpdatePreferencesRequest req);
	void deletePreference(String name);
	boolean isServiceUnavailable();
	void setServiceAvailability(boolean value);

}
