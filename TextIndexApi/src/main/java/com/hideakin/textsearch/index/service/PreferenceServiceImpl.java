package com.hideakin.textsearch.index.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.model.UpdatePreferencesRequest;
import com.hideakin.textsearch.index.model.ValueResponse;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@Service
public class PreferenceServiceImpl implements PreferenceService {

	@Autowired
	private PreferenceRepository preferenceRepository;

	@Override
	public ValueResponse getPreference(String name) {
		ValueResponse rsp = new ValueResponse();
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			rsp.setValue(entity.getValue());
		}
		return rsp;
	}

	@Override
	public void updatePreferences(UpdatePreferencesRequest req) {
		PreferenceEntity entity = new PreferenceEntity();
		entity.setName(req.getName());
		entity.setValue(req.getValue());
		preferenceRepository.save(entity);
	}

	@Override
	public void deletePreference(String name) {
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			preferenceRepository.delete(entity);
		}
	}
	
	@Override
	public boolean isServiceUnavailable() {
		ValueResponse rsp = getPreference("enabled");
		String value = rsp.getValue();
		return value != null && value.equalsIgnoreCase("false");
	}

	@Override
	public void setServiceAvailability(boolean value) {
		UpdatePreferencesRequest req = new UpdatePreferencesRequest("enabled", value ? "true" : "false");
		updatePreferences(req);
	}

}
