package com.hideakin.textsearch.index.service;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.model.NameValuePair;
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
		for (NameValuePair pref : req.getPrefs()) {
			PreferenceEntity entity = new PreferenceEntity();
			entity.setName(pref.getName());
			entity.setValue(pref.getValue());
			preferenceRepository.save(entity);
		}
	}

	@Override
	public void deletePreference(String name) {
		PreferenceEntity entity = preferenceRepository.findByName(name);
		if (entity != null) {
			preferenceRepository.delete(entity);
		}
	}

}
