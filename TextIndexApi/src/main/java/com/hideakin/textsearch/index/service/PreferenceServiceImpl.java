package com.hideakin.textsearch.index.service;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.model.UpdatePreferenceRequest;
import com.hideakin.textsearch.index.model.ValueResponse;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@Service
public class PreferenceServiceImpl implements PreferenceService {

	private static final Logger logger = LoggerFactory.getLogger(PreferenceServiceImpl.class);

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
	public void updatePreference(UpdatePreferenceRequest req) {
		preferenceRepository.save(new PreferenceEntity(req.getName(), req.getValue()));
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
		PreferenceEntity entity = preferenceRepository.findByName("enabled");
		return "false".equalsIgnoreCase(entity != null ? entity.getValue() : null);
	}

	@Override
	public void setServiceAvailability(boolean value) {
		preferenceRepository.save(new PreferenceEntity("enabled", value ? "true" : "false"));
		logger.warn("{}", value ? "Exited maintenance mode." : "Entered maintenance mode.");
	}

}
