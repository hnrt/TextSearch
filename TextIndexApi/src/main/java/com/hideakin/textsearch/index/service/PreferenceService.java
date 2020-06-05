package com.hideakin.textsearch.index.service;

import com.hideakin.textsearch.index.model.UpdatePreferenceRequest;
import com.hideakin.textsearch.index.model.ValueResponse;

public interface PreferenceService {

	ValueResponse getPreference(String name);
	void updatePreference(UpdatePreferenceRequest req);
	void deletePreference(String name);
	boolean isServiceUnavailable();
	void setServiceAvailability(boolean value);

}
